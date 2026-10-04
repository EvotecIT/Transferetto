using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Exposes a rooted local or mounted-network filesystem as a transfer endpoint.
/// </summary>
/// <remarks>
/// The root must be controlled by the caller's security context. This endpoint rejects symbolic links and
/// reparse points observed during path resolution, but it is not an operating-system sandbox against another
/// process that can mutate the directory tree concurrently. A privileged process must not use a root writable
/// by less-trusted identities.
/// </remarks>
public sealed class FileSystemTransferEndpoint : ITransferEndpoint, ITransferRangeEndpoint,
    ITransferDirectoryEndpoint, ITransferTimestampEndpoint {
    private readonly string _rootPath;
    private readonly StringComparison _pathComparison;

    /// <summary>Initializes a filesystem endpoint rooted beneath the supplied directory.</summary>
    public FileSystemTransferEndpoint(string rootPath) {
        if (string.IsNullOrWhiteSpace(rootPath)) {
            throw new ArgumentException("A root path is required.", nameof(rootPath));
        }

        _pathComparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        string fullRootPath = Path.GetFullPath(rootPath);
        string? volumeRoot = Path.GetPathRoot(fullRootPath);
        _rootPath = volumeRoot != null &&
                    string.Equals(
                        fullRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        volumeRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        _pathComparison)
            ? volumeRoot
            : fullRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <inheritdoc />
    public string Scheme => "file";

    /// <inheritdoc />
    public string DisplayName => new Uri(EnsureTrailingSeparator(_rootPath)).AbsoluteUri;

    internal string ResolveForResume(string path) => ResolvePath(path);

    /// <inheritdoc />
    public TransferEndpointCapabilities Capabilities =>
        TransferEndpointCapabilities.Inspect |
        TransferEndpointCapabilities.List |
        TransferEndpointCapabilities.Read |
        TransferEndpointCapabilities.Write |
        TransferEndpointCapabilities.Delete |
        TransferEndpointCapabilities.ConcurrentOperations |
        TransferEndpointCapabilities.RangeRead;

    /// <inheritdoc />
    public Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        if (!File.Exists(fullPath)) {
            return Task.FromResult<TransferItem?>(null);
        }
        return Task.FromResult(TryCreateItem(fullPath, out TransferItem? item) ? item : null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TransferItem>> ListAsync(
        string prefix,
        bool recursive = true,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(prefix, allowEmpty: true);
        if (File.Exists(fullPath)) {
            return Task.FromResult<IReadOnlyList<TransferItem>>(new[] { CreateItem(fullPath) });
        }
        if (!Directory.Exists(fullPath)) {
            return Task.FromResult<IReadOnlyList<TransferItem>>(Array.Empty<TransferItem>());
        }

        IReadOnlyList<TransferItem> items = EnumerateFilesSafely(
                fullPath,
                recursive,
                cancellationToken)
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .ToArray();
        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public Task<TransferReadHandle> OpenReadAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        FileInfo file = new(fullPath);
        if (!file.Exists) {
            throw new FileNotFoundException("The source item does not exist.", fullPath);
        }
        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(new TransferReadHandle(CreateItem(fullPath), stream));
    }

    /// <inheritdoc />
    public Task<TransferReadHandle> OpenReadRangeAsync(string path, long offset, long length,
        TransferItem expectedItem, CancellationToken cancellationToken = default) {
        if (offset < 0 || length <= 0 || expectedItem?.Length is not long sourceLength
            || offset > sourceLength - length) { throw new ArgumentOutOfRangeException(nameof(offset)); }
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        TransferItem current = CreateItem(fullPath);
        if (current.Length != expectedItem.Length || current.LastModifiedUtc != expectedItem.LastModifiedUtc) {
            throw new IOException("The source file changed before a ranged read.");
        }
        FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        try {
            stream.Seek(offset, SeekOrigin.Begin);
            return Task.FromResult(new TransferReadHandle(current, new BoundedReadStream(stream, length)));
        } catch { stream.Dispose(); throw; }
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

        TransferWriteOptions resolvedOptions = options ?? new TransferWriteOptions();
        string fullPath = ResolvePath(path);
        if (File.Exists(fullPath)) {
            if (resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
                return new TransferWriteResult(CreateItem(fullPath), wasWritten: false);
            }
            if (resolvedOptions.Mode == TransferWriteMode.FailIfExists) {
                throw new IOException($"The destination item already exists: {path}");
            }
        }

        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        string tempPath = fullPath + ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (FileStream target = new(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                TransferFileSystem.PreserveStagingPermissions(tempPath, fullPath);
                await TransferContent.CopyToAsync(content, target, length, cancellationToken).ConfigureAwait(false);
                await target.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            EnsureNoLinkTraversal(fullPath);
            if (resolvedOptions.Mode == TransferWriteMode.Overwrite) {
                CommitOverwrite(tempPath, fullPath);
            } else {
                try {
                    File.Move(tempPath, fullPath);
                } catch (IOException exception) when (File.Exists(fullPath)) {
                    EnsureNoLinkTraversal(fullPath);
                    if (resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
                        return new TransferWriteResult(CreateItem(fullPath), wasWritten: false);
                    }
                    throw new IOException($"The destination item already exists: {path}", exception);
                }
            }
            return new TransferWriteResult(
                CreateItem(
                    fullPath,
                    resolvedOptions.ContentType,
                    new Dictionary<string, string>(resolvedOptions.Metadata, StringComparer.OrdinalIgnoreCase)),
                wasWritten: true);
        } finally {
            if (File.Exists(tempPath)) {
                File.Delete(tempPath);
            }
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        if (!File.Exists(fullPath)) {
            return Task.FromResult(false);
        }
        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteEmptyDirectoryAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        if (!Directory.Exists(fullPath)) { return Task.FromResult(false); }
        Directory.Delete(fullPath, recursive: false);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task SetLastModifiedUtcAsync(string path, DateTimeOffset timestamp,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = ResolvePath(path);
        File.SetLastWriteTimeUtc(fullPath, timestamp.UtcDateTime);
        return Task.CompletedTask;
    }

    private TransferItem CreateItem(
        string fullPath,
        string? contentType = null,
        IReadOnlyDictionary<string, string>? metadata = null) {
        FileInfo file = new(fullPath);
        string relativePath = fullPath.Substring(_rootPath.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace(Path.DirectorySeparatorChar, '/');
        return new TransferItem {
            Path = relativePath,
            Length = file.Length,
            LastModifiedUtc = file.LastWriteTimeUtc,
            ContentType = contentType,
            Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
    }

    private bool TryCreateItem(string fullPath, out TransferItem? item) {
        try {
            item = CreateItem(fullPath);
            return true;
        } catch (FileNotFoundException) {
            item = null;
            return false;
        } catch (DirectoryNotFoundException) {
            item = null;
            return false;
        }
    }

    private string ResolvePath(string path, bool allowEmpty = false) {
        string relative = path ?? string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { relative = relative.Replace('\\', '/'); }
        return TransferFileSystem.ResolveRelativePath(_rootPath, relative, allowEmpty);
    }
    private IEnumerable<TransferItem> EnumerateFilesSafely(
        string directory,
        bool recursive,
        CancellationToken cancellationToken) {
        Stack<string> pending = new();
        pending.Push(directory);
        while (pending.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();
            string current = pending.Pop();
            EnsureNoLinkTraversal(current);
            foreach (string file in Directory.EnumerateFiles(current)) {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoLinkTraversal(file);
                yield return CreateItem(file);
            }
            if (!recursive) {
                continue;
            }
            foreach (string child in Directory.EnumerateDirectories(current)) {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoLinkTraversal(child);
                pending.Push(child);
            }
        }
    }

    private void EnsureNoLinkTraversal(string candidate) => TransferFileSystem.EnsureNoLinkTraversal(_rootPath, candidate);

    private void CommitOverwrite(string tempPath, string fullPath) => TransferFileSystem.CommitStagedFile(tempPath, fullPath);
    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
        path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
}
