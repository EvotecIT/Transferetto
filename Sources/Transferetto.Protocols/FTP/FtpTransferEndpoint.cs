using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Transferetto.Core;

namespace Transferetto;

/// <summary>
/// Exposes an FTP or FTPS session through the provider-neutral transfer endpoint contract.
/// </summary>
/// <remarks>
/// The configured prefix is a namespace boundary, not a security sandbox. Do not pass untrusted paths to a
/// privileged session when the remote server can expose symbolic links beneath that prefix.
/// </remarks>
public sealed class FtpTransferEndpoint : ITransferEndpoint, IDisposable {
    private readonly TransferettoFtpSession _session;
    private readonly string _prefix;
    private readonly bool _ownsSession;

    /// <summary>Initializes an endpoint over an existing connected FTP or FTPS session.</summary>
    public FtpTransferEndpoint(
        TransferettoFtpSession session,
        string? prefix = null,
        bool ownsSession = false) {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _prefix = ProtocolTransferEndpointPath.NormalizeRoot(prefix);
        _ownsSession = ownsSession;
    }

    /// <summary>Connects an FTP or FTPS session and initializes an endpoint that owns it.</summary>
    public FtpTransferEndpoint(TransferettoFtpConnectionOptions options, string? prefix = null)
        : this(ConnectOwnedSession(options, prefix), prefix, ownsSession: true) {
    }

    /// <inheritdoc />
    public string Scheme => _session.Client.Config.EncryptionMode == FtpEncryptionMode.None ? "ftp" : "ftps";

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
        TransferettoRemoteItem? item = TransferettoClient.GetFtpItem(
            _session,
            ProtocolTransferEndpointPath.Resolve(_prefix, relativePath));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(item?.Type == FtpObjectType.File ? ToTransferItem(relativePath, item) : null);
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
        if (!string.IsNullOrEmpty(remotePrefix) && !TransferettoClient.TestFtpDirectory(_session, remotePrefix)) {
            return Array.Empty<TransferItem>();
        }

        List<TransferItem> items = new();
        Queue<(string RemotePath, string RelativePath)> pending = new();
        pending.Enqueue((remotePrefix, relativePrefix));
        while (pending.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();
            (string remotePath, string relativePath) = pending.Dequeue();
            foreach (TransferettoRemoteItem item in TransferettoClient.GetFtpListing(_session, remotePath)) {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.Name == "." || item.Name == ".." || item.Type == FtpObjectType.Link) {
                    continue;
                }
                string itemRelativePath = ProtocolTransferEndpointPath.CombineRelative(relativePath, item.Name);
                if (item.Type == FtpObjectType.File) {
                    items.Add(ToTransferItem(itemRelativePath, item));
                } else if (recursive && item.Type == FtpObjectType.Directory) {
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
            throw new FileNotFoundException("The source FTP item does not exist.", relativePath);
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
                throw new IOException($"The destination FTP item already exists: {relativePath}");
            }
        }

        string? parent = ProtocolTransferEndpointPath.GetParent(remotePath);
        if (!string.IsNullOrEmpty(parent) && parent != "/") {
            TransferettoClient.CreateFtpDirectory(_session, parent!, force: true);
        }

        string temporaryPath = ProtocolTransferEndpointPath.CreateTemporaryPath(remotePath);
        try {
            using (Stream destination = _session.Client.OpenWrite(temporaryPath)) {
                await content.CopyToAsync(destination, 81920, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            FtpRemoteExists moveMode = resolvedOptions.Mode == TransferWriteMode.Overwrite
                ? FtpRemoteExists.Overwrite
                : FtpRemoteExists.Skip;
            TransferettoClient.MoveFtpFile(_session, temporaryPath, remotePath, moveMode);
            if (TransferettoClient.TestFtpFile(_session, temporaryPath)) {
                TransferettoClient.RemoveFtpFile(_session, temporaryPath);
                TransferItem? racedItem = await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false);
                if (resolvedOptions.Mode == TransferWriteMode.SkipIfExists && racedItem != null) {
                    return new TransferWriteResult(racedItem, wasWritten: false);
                }
                throw new IOException($"The destination FTP item already exists: {relativePath}");
            }

            TransferItem? written = await GetItemAsync(relativePath, cancellationToken).ConfigureAwait(false);
            return new TransferWriteResult(written ?? new TransferItem {
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
        TransferettoClient.RemoveFtpFile(_session, ProtocolTransferEndpointPath.Resolve(_prefix, relativePath));
        return true;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_ownsSession) {
            _session.Dispose();
        }
    }

    private void TryRemoveTemporaryFile(string path) {
        try {
            if (TransferettoClient.TestFtpFile(_session, path)) {
                TransferettoClient.RemoveFtpFile(_session, path);
            }
        } catch {
            // Preserve the original transfer failure when best-effort cleanup also fails.
        }
    }

    private static TransferettoFtpSession ConnectOwnedSession(
        TransferettoFtpConnectionOptions options,
        string? prefix) {
        _ = ProtocolTransferEndpointPath.NormalizeRoot(prefix);
        return TransferettoClient.ConnectFtp(options);
    }

    private static TransferItem ToTransferItem(string relativePath, TransferettoRemoteItem item) => new() {
        Path = relativePath,
        Length = ProtocolTransferEndpointPath.NormalizeLength(item.Size),
        LastModifiedUtc = item.Modified == default
            ? null
            : new DateTimeOffset(item.Modified.ToUniversalTime())
    };
}
