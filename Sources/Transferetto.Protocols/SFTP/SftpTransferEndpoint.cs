using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto;

/// <summary>
/// Exposes an SFTP session through the provider-neutral transfer endpoint contract.
/// </summary>
/// <remarks>
/// The configured prefix is a namespace boundary, not a security sandbox. Do not pass untrusted paths to a
/// privileged session when the remote server can expose symbolic links beneath that prefix.
/// </remarks>
public sealed class SftpTransferEndpoint : ITransferEndpoint, IDisposable {
    private readonly TransferettoSftpSession _session;
    private readonly string _prefix;
    private readonly bool _ownsSession;

    /// <summary>Initializes an endpoint over an existing connected SFTP session.</summary>
    public SftpTransferEndpoint(
        TransferettoSftpSession session,
        string? prefix = null,
        bool ownsSession = false)
        : this(
            session,
            prefix,
            ownsSession,
            TransferettoClient.GetSftpWorkingDirectory(session ?? throw new ArgumentNullException(nameof(session)))) {
    }

    internal SftpTransferEndpoint(
        TransferettoSftpSession session,
        string? prefix,
        bool ownsSession,
        string workingDirectory) {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _prefix = ProtocolTransferEndpointPath.AnchorRoot(prefix, workingDirectory);
        _ownsSession = ownsSession;
    }

    /// <summary>Connects an SFTP session and initializes an endpoint that owns it.</summary>
    public SftpTransferEndpoint(TransferettoSftpConnectionOptions options, string? prefix = null)
        : this(ConnectOwnedEndpoint(options, prefix)) {
    }

    /// <inheritdoc />
    public string Scheme => "sftp";

    /// <inheritdoc />
    public string DisplayName => new UriBuilder(
        Scheme,
        _session.Host,
        _session.Port,
        string.IsNullOrEmpty(_prefix) ? "/" : "/" + _prefix.TrimStart('/')).Uri.AbsoluteUri;

    /// <inheritdoc />
    public TransferEndpointCapabilities Capabilities =>
        TransferEndpointCapabilities.Inspect |
        TransferEndpointCapabilities.List |
        TransferEndpointCapabilities.Read |
        TransferEndpointCapabilities.Write |
        TransferEndpointCapabilities.Delete;

    /// <inheritdoc />
    public Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string relativePath = ProtocolTransferEndpointPath.NormalizeRelative(path);
        string remotePath = ProtocolTransferEndpointPath.Resolve(_prefix, relativePath);
        if (!TransferettoClient.TestSftpFile(_session, remotePath)) {
            return Task.FromResult<TransferItem?>(null);
        }
        TransferettoSftpAttributes attributes = TransferettoClient.GetSftpAttributes(_session, remotePath);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<TransferItem?>(ToTransferItem(relativePath, attributes));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransferItem>> ListAsync(
        string prefix,
        bool recursive = true,
        CancellationToken cancellationToken = default) {
        string relativePrefix = ProtocolTransferEndpointPath.NormalizeRelative(prefix, allowEmpty: true);
        if (!string.IsNullOrEmpty(relativePrefix)) {
            TransferItem? item = await GetItemAsync(relativePrefix, cancellationToken).ConfigureAwait(false);
            if (item != null) {
                return new[] { item };
            }
        }

        string remotePrefix = ProtocolTransferEndpointPath.Resolve(_prefix, relativePrefix, allowEmpty: true);
        if (!string.IsNullOrEmpty(remotePrefix) && !TransferettoClient.TestSftpDirectory(_session, remotePrefix)) {
            return Array.Empty<TransferItem>();
        }

        List<TransferItem> items = new();
        Queue<(string RemotePath, string RelativePath)> pending = new();
        pending.Enqueue((string.IsNullOrEmpty(remotePrefix) ? "." : remotePrefix, relativePrefix));
        while (pending.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();
            (string remotePath, string relativePath) = pending.Dequeue();
            foreach (TransferettoSftpItem item in TransferettoClient.GetSftpListing(_session, remotePath)) {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.Name == "." || item.Name == ".." || item.IsSymbolicLink) {
                    continue;
                }
                string itemRelativePath = ProtocolTransferEndpointPath.CombineRelative(relativePath, item.Name);
                if (item.IsRegularFile) {
                    items.Add(new TransferItem {
                        Path = itemRelativePath,
                        Length = ProtocolTransferEndpointPath.NormalizeLength(item.Length),
                        LastModifiedUtc = item.LastWriteTime == default
                            ? null
                            : new DateTimeOffset(item.LastWriteTime.ToUniversalTime())
                    });
                } else if (recursive && item.IsDirectory) {
                    pending.Enqueue((item.FullName, itemRelativePath));
                }
            }
            if (!recursive) {
                break;
            }
        }
        items.Sort((left, right) => StringComparer.Ordinal.Compare(left.Path, right.Path));
        return items;
    }

    /// <inheritdoc />
    public async Task<TransferReadHandle> OpenReadAsync(
        string path,
        CancellationToken cancellationToken = default) {
        string relativePath = ProtocolTransferEndpointPath.NormalizeRelative(path);
        TransferItem? item = await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false);
        if (item == null) {
            throw new FileNotFoundException("The source SFTP item does not exist.", relativePath);
        }
        return ProtocolTransferEndpointResource.OpenRead(
            () => _session.Client.OpenRead(ProtocolTransferEndpointPath.Resolve(_prefix, relativePath)),
            item,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TransferWriteResult> WriteAsync(
        string path,
        Stream content,
        long? length,
        TransferWriteOptions? options = null,
        CancellationToken cancellationToken = default) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        string relativePath = ProtocolTransferEndpointPath.NormalizeRelative(path);
        string remotePath = ProtocolTransferEndpointPath.Resolve(_prefix, relativePath);
        TransferWriteOptions resolvedOptions = options ?? new TransferWriteOptions();
        ProtocolTransferEndpointPath.ValidateUnsupportedMetadata(resolvedOptions, Scheme);

        TransferItem? existing = await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false);
        if (existing != null) {
            if (resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
            if (resolvedOptions.Mode == TransferWriteMode.FailIfExists) {
                throw new IOException($"The destination SFTP item already exists: {relativePath}");
            }
        }

        EnsureParentDirectory(remotePath);
        string temporaryPath = ProtocolTransferEndpointPath.CreateTemporaryPath(remotePath);
        try {
            using (Stream destination = _session.Client.OpenWrite(temporaryPath)) {
                await TransferContent.CopyToAsync(content, destination, length, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            bool committed = ProtocolTransferCommit.Commit(
                new SftpTransferCommitOperations(_session),
                temporaryPath,
                remotePath,
                relativePath,
                resolvedOptions.Mode,
                "SFTP");
            if (!committed) {
                TransferItem? racedItem = await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false);
                if (racedItem != null) {
                    return new TransferWriteResult(racedItem, wasWritten: false);
                }
                throw new IOException($"The destination SFTP item was created concurrently but is no longer available: {relativePath}");
            }

            return new TransferWriteResult(new TransferItem {
                Path = relativePath,
                Length = length,
                LastModifiedUtc = DateTimeOffset.UtcNow
            }, wasWritten: true);
        } catch {
            TryRemoveTemporaryFile(temporaryPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) {
        string relativePath = ProtocolTransferEndpointPath.NormalizeRelative(path);
        if (await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false) == null) {
            return false;
        }
        TransferettoClient.RemoveSftpFile(_session, ProtocolTransferEndpointPath.Resolve(_prefix, relativePath));
        return true;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_ownsSession) {
            _session.Dispose();
        }
    }

    private void EnsureParentDirectory(string remotePath) {
        string? parent = ProtocolTransferEndpointPath.GetParent(remotePath);
        if (string.IsNullOrEmpty(parent) || parent == "/") {
            return;
        }

        SftpTransferDirectory.Ensure(
            new SftpTransferDirectoryOperations(_session),
            parent!);
    }

    private void TryRemoveTemporaryFile(string path) {
        try {
            if (_session.Client.Exists(path)) {
                TransferettoClient.RemoveSftpFile(_session, path);
            }
        } catch {
            // Preserve the original transfer failure when best-effort cleanup also fails.
        }
    }

    private SftpTransferEndpoint(OwnedEndpointState state) {
        _session = state.Session;
        _prefix = state.Root;
        _ownsSession = true;
    }

    private static OwnedEndpointState ConnectOwnedEndpoint(
        TransferettoSftpConnectionOptions options,
        string? prefix) {
        _ = ProtocolTransferEndpointPath.NormalizeRoot(prefix);
        TransferettoSftpSession session = TransferettoClient.ConnectSftp(options);
        try {
            return new OwnedEndpointState(
                session,
                ProtocolTransferEndpointPath.AnchorRoot(
                    prefix,
                    TransferettoClient.GetSftpWorkingDirectory(session)));
        } catch {
            session.Dispose();
            throw;
        }
    }

    private sealed class OwnedEndpointState {
        internal OwnedEndpointState(
            TransferettoSftpSession session,
            string root) {
            Session = session;
            Root = root;
        }

        internal TransferettoSftpSession Session { get; }
        internal string Root { get; }
    }

    private static TransferItem ToTransferItem(string relativePath, TransferettoSftpAttributes attributes) => new() {
        Path = relativePath,
        Length = ProtocolTransferEndpointPath.NormalizeLength(attributes.Size),
        LastModifiedUtc = attributes.LastWriteTimeUtc == default
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(attributes.LastWriteTimeUtc, DateTimeKind.Utc))
    };
}
