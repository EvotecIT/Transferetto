using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentFTP;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>
    /// Synchronizes a local directory with an FTP or FTPS directory using a reusable Transferetto sync plan.
    /// </summary>
    public static IReadOnlyList<TransferettoSyncResult> SyncFtpDirectory(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        TransferettoSyncOptions? syncOptions = null,
        TransferettoTransferOptions? transferOptions = null) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        TransferettoSyncOptions resolvedSyncOptions = syncOptions ?? new TransferettoSyncOptions();
        TransferettoTransferOptions resolvedTransferOptions = transferOptions ?? new TransferettoTransferOptions();
        string normalizedRemotePath = NormalizeRemotePath(remotePath);
        bool remoteRootIsDirectory = session.Client.DirectoryExists(normalizedRemotePath);
        bool remoteRootIsFile = !remoteRootIsDirectory && session.Client.FileExists(normalizedRemotePath);

        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && !remoteRootIsDirectory) {
            if (remoteRootIsFile) {
                throw new InvalidOperationException($"Remote path {normalizedRemotePath} exists but is not a directory.");
            }

            throw new DirectoryNotFoundException($"Remote directory {normalizedRemotePath} does not exist.");
        }

        IReadOnlyList<TransferettoSyncEntry> localManifest = resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload
            ? BuildLocalSyncManifest(localPath, normalizedRemotePath)
            : BuildLocalSyncManifestOrEmpty(localPath, normalizedRemotePath);
        IReadOnlyList<TransferettoSyncEntry> remoteManifest = remoteRootIsDirectory
            ? BuildFtpRemoteSyncManifest(session, normalizedRemotePath, localPath)
            : Array.Empty<TransferettoSyncEntry>();
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload ? localManifest : remoteManifest,
            resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload ? remoteManifest : localManifest,
            resolvedSyncOptions);
        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload && remoteRootIsFile) {
            plan = HandleConflictingUploadRootFile(plan, localPath, normalizedRemotePath, resolvedSyncOptions);
        } else if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload && !remoteRootIsDirectory) {
            plan = HandleMissingUploadRoot(plan, localPath, normalizedRemotePath, resolvedSyncOptions);
        }

        bool localRootIsFile = resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && File.Exists(localPath);
        bool localRootExists = resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && Directory.Exists(localPath);
        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && !localRootExists && !localRootIsFile) {
            plan = HandleMissingDownloadRoot(plan, localPath, normalizedRemotePath, resolvedSyncOptions);
        }

        plan = HandleConflictingDownloadRootFile(plan, localPath, normalizedRemotePath, resolvedSyncOptions);

        if (resolvedSyncOptions.DryRun) {
            return ExecuteDryRunSyncPlan(plan);
        }

        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && resolvedSyncOptions.CreateDestinationDirectories && !localRootIsFile) {
            Directory.CreateDirectory(localPath);
        }

        List<TransferettoSyncResult> results = new();
        foreach (TransferettoSyncPlanItem item in plan) {
            resolvedTransferOptions.CancellationToken.ThrowIfCancellationRequested();
            TransferettoSyncResult result = ExecuteFtpSyncPlanItem(session, item, resolvedSyncOptions, resolvedTransferOptions);
            results.Add(result);
            if (ShouldStopMirrorAfterFileTransferFailure(item, result, resolvedSyncOptions)) {
                break;
            }
        }

        return results;
    }

    private static TransferettoSyncResult ExecuteFtpSyncPlanItem(
        TransferettoFtpSession session,
        TransferettoSyncPlanItem item,
        TransferettoSyncOptions syncOptions,
        TransferettoTransferOptions transferOptions) {
        if (item.Action == TransferettoSyncAction.Skip) {
            return CreateSyncResult(item, true, false, true, null, item.Message);
        }

        switch (item.Action) {
            case TransferettoSyncAction.CreateDirectory:
                if (syncOptions.Direction == TransferettoSyncDirection.Upload) {
                    CreateFtpDirectory(session, item.RemotePath!, true);
                } else {
                    Directory.CreateDirectory(item.LocalPath!);
                }

                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.UploadFile:
                IReadOnlyList<TransferettoTransferResult> uploadResults = UploadFtpFiles(
                    session,
                    item.RemotePath,
                    new[] { item.LocalPath! },
                    null,
                    syncOptions.OverwriteExisting ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                    FtpVerify.None,
                    FtpError.Throw,
                    syncOptions.CreateDestinationDirectories,
                    transferOptions);
                TransferettoTransferResult uploadResult = uploadResults.Count > 0 ? uploadResults[0] : EmptyTransferResult(item);
                if (syncOptions.PreserveTimestamps && IsCompletedFileTransfer(uploadResult) && item.Source?.LastWriteTimeUtc is DateTime uploadTimestamp) {
                    SetFtpModifiedTime(session, item.RemotePath!, uploadTimestamp);
                }

                return CreateSyncResult(item, uploadResult);
            case TransferettoSyncAction.DownloadFile:
                TransferettoTransferResult downloadResult = DownloadFtpFile(
                    session,
                    item.LocalPath!,
                    item.RemotePath!,
                    syncOptions.OverwriteExisting ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                    FtpVerify.None,
                    transferOptions);
                if (syncOptions.PreserveTimestamps && IsCompletedFileTransfer(downloadResult) && item.Source?.LastWriteTimeUtc is DateTime downloadTimestamp && File.Exists(item.LocalPath)) {
                    File.SetLastWriteTimeUtc(item.LocalPath!, downloadTimestamp);
                }

                return CreateSyncResult(item, downloadResult);
            case TransferettoSyncAction.DeleteRemoteFile:
                RemoveFtpFile(session, item.RemotePath!);
                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.DeleteRemoteDirectory:
                RemoveFtpDirectory(session, item.RemotePath!);
                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.DeleteLocalFile:
                File.Delete(item.LocalPath!);
                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.DeleteLocalDirectory:
                Directory.Delete(item.LocalPath!);
                return CreateSyncResult(item, true, false, false, null, item.Message);
            default:
                return CreateSyncResult(item, true, false, true, null, item.Message);
        }
    }

    private static IReadOnlyList<TransferettoSyncEntry> BuildFtpRemoteSyncManifest(
        TransferettoFtpSession session,
        string remoteRoot,
        string localRoot) {
        List<TransferettoSyncEntry> entries = new();
        BuildFtpRemoteSyncManifest(session, remoteRoot, remoteRoot, localRoot, entries);
        return entries;
    }

    private static void BuildFtpRemoteSyncManifest(
        TransferettoFtpSession session,
        string remoteRoot,
        string currentRemotePath,
        string localRoot,
        ICollection<TransferettoSyncEntry> entries) {
        foreach (TransferettoRemoteItem item in GetFtpListing(session, currentRemotePath)) {
            if (IsSpecialRemoteDirectoryName(item.Name)) {
                continue;
            }

            string relativePath = GetRemoteRelativePath(remoteRoot, item.FullName);
            string localPath = Path.Combine(localRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            bool isDirectory = item.Type == FtpObjectType.Directory;
            bool isFile = item.Type == FtpObjectType.File;
            if (!isDirectory && !isFile) {
                continue;
            }

            entries.Add(new TransferettoSyncEntry {
                RelativePath = relativePath,
                LocalPath = localPath,
                RemotePath = item.FullName,
                IsDirectory = isDirectory,
                Length = isFile ? item.Size : null,
                LastWriteTimeUtc = ToUtcTimestamp(item.Modified)
            });

            if (isDirectory) {
                BuildFtpRemoteSyncManifest(session, remoteRoot, item.FullName, localRoot, entries);
            }
        }
    }

    private static TransferettoTransferResult EmptyTransferResult(TransferettoSyncPlanItem item) {
        return new TransferettoTransferResult {
            Action = item.Action.ToString(),
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = item.LocalPath,
            RemotePath = item.RemotePath,
            Message = item.Message
        };
    }
}
