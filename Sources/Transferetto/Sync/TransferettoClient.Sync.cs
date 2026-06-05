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
        if (Path.DirectorySeparatorChar == '\\') {
            relativePath = relativePath.Replace('\\', '/');
        }

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
        string normalizedPath = NormalizeRemoteManifestPath(remotePath);
        if (normalizedRoot == string.Empty || normalizedRoot == "/") {
            return TransferettoSyncPlanner.NormalizeRelativePath(normalizedPath);
        }

        if (normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase)) {
            return TransferettoSyncPlanner.NormalizeRelativePath(normalizedPath.Substring(normalizedRoot.Length + 1));
        }

        return TransferettoSyncPlanner.NormalizeRelativePath(GetRemoteManifestName(normalizedPath));
    }

    private static string NormalizeRemoteManifestPath(string path) {
        return path.Length > 1
            ? path.TrimEnd('/')
            : path;
    }

    private static string GetRemoteManifestName(string path) {
        if (string.IsNullOrWhiteSpace(path) || path == "/") {
            return path;
        }

        int separatorIndex = path.LastIndexOf('/');
        return separatorIndex < 0
            ? path
            : path.Substring(separatorIndex + 1);
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

    private static IReadOnlyList<TransferettoSyncPlanItem> HandleMissingUploadRoot(
        IReadOnlyList<TransferettoSyncPlanItem> plan,
        string localPath,
        string remotePath,
        TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Upload) {
            return plan;
        }

        if (!options.CreateDestinationDirectories) {
            return plan
                .Select(item => item.Action == TransferettoSyncAction.UploadFile
                    ? new TransferettoSyncPlanItem {
                        Action = TransferettoSyncAction.Skip,
                        Direction = item.Direction,
                        RelativePath = item.RelativePath,
                        LocalPath = item.LocalPath,
                        RemotePath = item.RemotePath,
                        Source = item.Source,
                        Destination = item.Destination,
                        Message = "Destination root directory is missing and directory creation is disabled."
                    }
                    : item)
                .ToArray();
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

    private static IReadOnlyList<TransferettoSyncPlanItem> HandleMissingDownloadRoot(
        IReadOnlyList<TransferettoSyncPlanItem> plan,
        string localPath,
        string remotePath,
        TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Download || options.CreateDestinationDirectories) {
            return plan;
        }

        return plan
            .Select(item => item.Action == TransferettoSyncAction.DownloadFile
                ? new TransferettoSyncPlanItem {
                    Action = TransferettoSyncAction.Skip,
                    Direction = item.Direction,
                    RelativePath = item.RelativePath,
                    LocalPath = item.LocalPath,
                    RemotePath = item.RemotePath,
                    Source = item.Source,
                    Destination = item.Destination,
                    Message = "Destination root directory is missing and directory creation is disabled."
                }
                : item)
            .ToArray();
    }

    private static IReadOnlyList<TransferettoSyncPlanItem> HandleConflictingUploadRootFile(
        IReadOnlyList<TransferettoSyncPlanItem> plan,
        string localPath,
        string remotePath,
        TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Upload) {
            return plan;
        }

        TransferettoSyncEntry sourceRoot = CreateRootSyncEntry(localPath, remotePath, isDirectory: true);
        TransferettoSyncEntry destinationRoot = CreateRootSyncEntry(localPath, remotePath, isDirectory: false);
        if (!options.CreateDestinationDirectories || !options.OverwriteExisting) {
            return new[] {
                CreateRootSyncPlanItem(
                    TransferettoSyncAction.Skip,
                    options.Direction,
                    sourceRoot,
                    destinationRoot,
                    "Destination root path is a file and cannot be replaced.")
            };
        }

        return new[] {
            CreateRootSyncPlanItem(
                TransferettoSyncAction.DeleteRemoteFile,
                options.Direction,
                sourceRoot,
                destinationRoot,
                "Destination root file conflicts with source directory."),
            CreateRootSyncPlanItem(
                TransferettoSyncAction.CreateDirectory,
                options.Direction,
                sourceRoot,
                null,
                "Destination root directory replaces conflicting file.")
        }.Concat(plan).ToArray();
    }

    private static IReadOnlyList<TransferettoSyncPlanItem> HandleConflictingDownloadRootFile(
        IReadOnlyList<TransferettoSyncPlanItem> plan,
        string localPath,
        string remotePath,
        TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Download || !File.Exists(localPath)) {
            return plan;
        }

        TransferettoSyncEntry sourceRoot = CreateRootSyncEntry(localPath, remotePath, isDirectory: true);
        TransferettoSyncEntry destinationRoot = CreateRootSyncEntry(localPath, remotePath, isDirectory: false);
        if (!options.CreateDestinationDirectories || !options.OverwriteExisting) {
            return new[] {
                CreateRootSyncPlanItem(
                    TransferettoSyncAction.Skip,
                    options.Direction,
                    sourceRoot,
                    destinationRoot,
                    "Destination root path is a file and cannot be replaced.")
            };
        }

        return new[] {
            CreateRootSyncPlanItem(
                TransferettoSyncAction.DeleteLocalFile,
                options.Direction,
                sourceRoot,
                destinationRoot,
                "Destination root file conflicts with source directory."),
            CreateRootSyncPlanItem(
                TransferettoSyncAction.CreateDirectory,
                options.Direction,
                sourceRoot,
                null,
                "Destination root directory replaces conflicting file.")
        }.Concat(plan).ToArray();
    }

    private static TransferettoSyncEntry CreateRootSyncEntry(string localPath, string remotePath, bool isDirectory) {
        return new TransferettoSyncEntry {
            RelativePath = string.Empty,
            LocalPath = localPath,
            RemotePath = remotePath,
            IsDirectory = isDirectory
        };
    }

    private static TransferettoSyncPlanItem CreateRootSyncPlanItem(
        TransferettoSyncAction action,
        TransferettoSyncDirection direction,
        TransferettoSyncEntry? source,
        TransferettoSyncEntry? destination,
        string message) {
        TransferettoSyncEntry? item = source ?? destination;
        return new TransferettoSyncPlanItem {
            Action = action,
            Direction = direction,
            RelativePath = string.Empty,
            LocalPath = item?.LocalPath,
            RemotePath = item?.RemotePath,
            Source = source,
            Destination = destination,
            Message = message
        };
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
