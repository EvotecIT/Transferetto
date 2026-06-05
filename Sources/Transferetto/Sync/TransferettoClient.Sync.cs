using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Transferetto;

public static partial class TransferettoClient {
    private static IReadOnlyList<TransferettoSyncEntry> BuildLocalSyncManifest(string localRoot, string remoteRoot) {
        EnsureNotNullOrWhiteSpace(localRoot, nameof(localRoot));
        DirectoryInfo root = new(localRoot);
        if (!root.Exists) {
            throw new DirectoryNotFoundException($"Directory {localRoot} does not exist.");
        }

        List<TransferettoSyncEntry> entries = new();
        foreach (DirectoryInfo directory in root.GetDirectories("*", SearchOption.AllDirectories)) {
            string relativePath = GetLocalRelativePath(root.FullName, directory.FullName);
            entries.Add(new TransferettoSyncEntry {
                RelativePath = relativePath,
                LocalPath = directory.FullName,
                RemotePath = CombineSyncRemotePath(remoteRoot, relativePath),
                IsDirectory = true,
                LastWriteTimeUtc = directory.LastWriteTimeUtc
            });
        }

        foreach (FileInfo file in root.GetFiles("*", SearchOption.AllDirectories)) {
            string relativePath = GetLocalRelativePath(root.FullName, file.FullName);
            entries.Add(new TransferettoSyncEntry {
                RelativePath = relativePath,
                LocalPath = file.FullName,
                RemotePath = CombineSyncRemotePath(remoteRoot, relativePath),
                IsDirectory = false,
                Length = file.Length,
                LastWriteTimeUtc = file.LastWriteTimeUtc
            });
        }

        return entries;
    }

    private static IReadOnlyList<TransferettoSyncEntry> BuildLocalSyncManifestOrEmpty(string localRoot, string remoteRoot) {
        return Directory.Exists(localRoot)
            ? BuildLocalSyncManifest(localRoot, remoteRoot)
            : Array.Empty<TransferettoSyncEntry>();
    }

    private static string GetLocalRelativePath(string rootPath, string fullPath) {
        string normalizedRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string normalizedPath = Path.GetFullPath(fullPath);
        string relativePath = normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            ? normalizedPath.Substring(normalizedRoot.Length)
            : Path.GetFileName(normalizedPath);
        return TransferettoSyncPlanner.NormalizeRelativePath(relativePath);
    }

    private static string CombineSyncRemotePath(string basePath, string relativePath) {
        string normalizedBasePath = NormalizeRemotePath(basePath);
        string normalizedRelativePath = TransferettoSyncPlanner.NormalizeRelativePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalizedRelativePath)) {
            return normalizedBasePath;
        }

        return normalizedBasePath == "/"
            ? "/" + normalizedRelativePath
            : normalizedBasePath.TrimEnd('/') + "/" + normalizedRelativePath;
    }

    private static string GetRemoteRelativePath(string remoteRoot, string remotePath) {
        string normalizedRoot = NormalizeRemotePath(remoteRoot).TrimEnd('/');
        string normalizedPath = NormalizeRemotePath(remotePath);
        if (normalizedRoot == string.Empty || normalizedRoot == "/") {
            return TransferettoSyncPlanner.NormalizeRelativePath(normalizedPath);
        }

        if (normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase)) {
            return TransferettoSyncPlanner.NormalizeRelativePath(normalizedPath.Substring(normalizedRoot.Length + 1));
        }

        return TransferettoSyncPlanner.NormalizeRelativePath(GetRemoteName(normalizedPath));
    }

    private static DateTime? ToUtcTimestamp(DateTime value) {
        if (value == DateTime.MinValue || value == DateTime.MaxValue) {
            return null;
        }

        return value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }

    private static TransferettoSyncResult CreateDryRunSyncResult(TransferettoSyncPlanItem item) {
        return CreateSyncResult(item, true, true, item.Action == TransferettoSyncAction.Skip, null, item.Message);
    }

    private static TransferettoSyncResult CreateSyncResult(
        TransferettoSyncPlanItem item,
        bool status,
        bool isDryRun,
        bool isSkipped,
        long? bytesTransferred,
        string message) {
        return new TransferettoSyncResult {
            Action = item.Action,
            Direction = item.Direction,
            Status = status,
            IsDryRun = isDryRun,
            IsSkipped = isSkipped,
            RelativePath = item.RelativePath,
            LocalPath = item.LocalPath,
            RemotePath = item.RemotePath,
            BytesTransferred = bytesTransferred,
            Message = message
        };
    }

    private static TransferettoSyncResult CreateSyncResult(TransferettoSyncPlanItem item, TransferettoTransferResult transferResult) {
        return CreateSyncResult(
            item,
            transferResult.Status,
            false,
            transferResult.IsSkipped,
            transferResult.BytesTransferred,
            transferResult.Message);
    }

    private static bool IsSpecialRemoteDirectoryName(string name) {
        return string.Equals(name, ".", StringComparison.Ordinal) || string.Equals(name, "..", StringComparison.Ordinal);
    }

    private static IReadOnlyList<TransferettoSyncResult> ExecuteDryRunSyncPlan(IEnumerable<TransferettoSyncPlanItem> plan) {
        return plan.Select(CreateDryRunSyncResult).ToArray();
    }

    private static IReadOnlyList<TransferettoSyncPlanItem> PrependDestinationRootCreate(
        IReadOnlyList<TransferettoSyncPlanItem> plan,
        string localPath,
        string remotePath,
        TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Upload || !options.CreateDestinationDirectories) {
            return plan;
        }

        TransferettoSyncEntry sourceRoot = new() {
            RelativePath = string.Empty,
            LocalPath = localPath,
            RemotePath = remotePath,
            IsDirectory = true
        };
        TransferettoSyncPlanItem rootCreate = new() {
            Action = TransferettoSyncAction.CreateDirectory,
            Direction = options.Direction,
            RelativePath = string.Empty,
            LocalPath = localPath,
            RemotePath = remotePath,
            Source = sourceRoot,
            Message = "Destination root directory is missing."
        };

        return new[] { rootCreate }.Concat(plan).ToArray();
    }

    private static bool IsCompletedFileTransfer(TransferettoTransferResult result) {
        return result.IsSuccess && !result.IsSkipped && !result.IsFailed;
    }

    private static bool ShouldStopMirrorAfterFileTransferFailure(
        TransferettoSyncPlanItem item,
        TransferettoSyncResult result,
        TransferettoSyncOptions options) {
        return options.Mode == TransferettoSyncMode.Mirror
            && !result.Status
            && (item.Action == TransferettoSyncAction.UploadFile || item.Action == TransferettoSyncAction.DownloadFile);
    }
}
