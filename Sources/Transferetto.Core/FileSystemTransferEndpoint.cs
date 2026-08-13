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
public sealed class FileSystemTransferEndpoint : ITransferEndpoint {
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
        string fullPath = ResolvePath(path);
        if (!File.Exists(fullPath)) {
            return Task.FromResult<TransferItem?>(null);
        }
        return Task.FromResult<TransferItem?>(CreateItem(fullPath));
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
                await TransferContent.CopyToAsync(content, target, length, cancellationToken).ConfigureAwait(false);
                await target.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

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

    private string ResolvePath(string path, bool allowEmpty = false) {
        if (!allowEmpty && string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("An endpoint-relative path is required.", nameof(path));
        }
        if (Path.IsPathRooted(path ?? string.Empty)) {
            throw new ArgumentException("The path must be relative to the endpoint root.", nameof(path));
        }

        string candidate = Path.GetFullPath(Path.Combine(_rootPath, path ?? string.Empty));
        string rootWithSeparator = EnsureTrailingSeparator(_rootPath);
        if (!candidate.Equals(_rootPath, _pathComparison) &&
            !candidate.StartsWith(rootWithSeparator, _pathComparison)) {
            throw new ArgumentException("The path escapes the endpoint root.", nameof(path));
        }
        EnsureNoLinkTraversal(candidate);
        return candidate;
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

    private void EnsureNoLinkTraversal(string candidate) {
        if (candidate.Equals(_rootPath, _pathComparison)) {
            return;
        }

        string relativePath = candidate.Substring(_rootPath.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string current = _rootPath;
        foreach (string segment in relativePath.Split(
                     new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                     StringSplitOptions.RemoveEmptyEntries)) {
            current = Path.Combine(current, segment);
            if (!TryGetAttributes(current, out FileAttributes attributes)) {
                continue;
            }
            if ((attributes & FileAttributes.ReparsePoint) != 0) {
                throw new IOException(
                    $"The endpoint does not allow symbolic links or reparse points beneath its root: {candidate}");
            }
        }
    }

    private static bool TryGetAttributes(string path, out FileAttributes attributes) {
        try {
            attributes = File.GetAttributes(path);
            return true;
        } catch (FileNotFoundException) {
            attributes = default;
            return false;
        } catch (DirectoryNotFoundException) {
            attributes = default;
            return false;
        }
    }

    private void CommitOverwrite(string tempPath, string fullPath) {
        if (!File.Exists(fullPath)) {
            try {
                File.Move(tempPath, fullPath);
                return;
            } catch (IOException) when (File.Exists(fullPath)) {
                // The destination appeared after the check. Replace it below.
            }
        }
        EnsureNoLinkTraversal(fullPath);
        File.Replace(tempPath, fullPath, null);
    }

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
        path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
}
