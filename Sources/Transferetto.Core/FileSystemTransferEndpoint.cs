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

        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        IReadOnlyList<TransferItem> items = Directory
            .EnumerateFiles(fullPath, "*", searchOption)
            .Select(file => {
                cancellationToken.ThrowIfCancellationRequested();
                return CreateItem(file);
            })
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
                await content.CopyToAsync(target, 81920, cancellationToken).ConfigureAwait(false);
                await target.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(fullPath)) {
                File.Replace(tempPath, fullPath, null);
            } else {
                File.Move(tempPath, fullPath);
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
        return candidate;
    }

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
        path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
}
