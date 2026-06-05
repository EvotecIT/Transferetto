using System;
using System.Collections.Generic;
using System.IO;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>
    /// Synchronizes a local directory with an SFTP directory using a reusable Transferetto sync plan.
    /// </summary>
    public static IReadOnlyList<TransferettoSyncResult> SyncSftpDirectory(
        TransferettoSftpSession session,
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

        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Download
            && (!session.Client.Exists(normalizedRemotePath) || !session.Client.GetAttributes(normalizedRemotePath).IsDirectory)) {
            throw new DirectoryNotFoundException($"Remote directory {normalizedRemotePath} does not exist.");
        }

        if (resolvedSyncOptions.Direction == TransferettoSyncDirection.Download && resolvedSyncOptions.CreateDestinationDirectories) {
            Directory.CreateDirectory(localPath);
        }

        IReadOnlyList<TransferettoSyncEntry> localManifest = resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload
            ? BuildLocalSyncManifest(localPath, normalizedRemotePath)
            : BuildLocalSyncManifestOrEmpty(localPath, normalizedRemotePath);
        IReadOnlyList<TransferettoSyncEntry> remoteManifest = session.Client.Exists(normalizedRemotePath)
            ? BuildSftpRemoteSyncManifest(session, normalizedRemotePath, localPath)
            : Array.Empty<TransferettoSyncEntry>();
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload ? localManifest : remoteManifest,
            resolvedSyncOptions.Direction == TransferettoSyncDirection.Upload ? remoteManifest : localManifest,
            resolvedSyncOptions);

        if (resolvedSyncOptions.DryRun) {
            return ExecuteDryRunSyncPlan(plan);
        }

        List<TransferettoSyncResult> results = new();
        foreach (TransferettoSyncPlanItem item in plan) {
            resolvedTransferOptions.CancellationToken.ThrowIfCancellationRequested();
            results.Add(ExecuteSftpSyncPlanItem(session, item, resolvedSyncOptions, resolvedTransferOptions));
        }

        return results;
    }

    private static TransferettoSyncResult ExecuteSftpSyncPlanItem(
        TransferettoSftpSession session,
        TransferettoSyncPlanItem item,
        TransferettoSyncOptions syncOptions,
        TransferettoTransferOptions transferOptions) {
        if (item.Action == TransferettoSyncAction.Skip) {
            return CreateSyncResult(item, true, false, true, null, item.Message);
        }

        switch (item.Action) {
            case TransferettoSyncAction.CreateDirectory:
                if (syncOptions.Direction == TransferettoSyncDirection.Upload) {
                    EnsureSftpDirectoryExists(session, item.RemotePath!, null, transferOptions);
                } else {
                    Directory.CreateDirectory(item.LocalPath!);
                }

                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.UploadFile:
                if (syncOptions.CreateDestinationDirectories) {
                    EnsureSftpDirectoryExists(session, GetRemoteParent(item.RemotePath!), null, transferOptions);
                }

                TransferettoTransferResult uploadResult = UploadSftpFile(session, item.LocalPath!, item.RemotePath!, syncOptions.OverwriteExisting, transferOptions);
                if (syncOptions.PreserveTimestamps && item.Source?.LastWriteTimeUtc is DateTime uploadTimestamp) {
                    SetSftpTimestamp(session, item.RemotePath!, null, uploadTimestamp, useUtc: true);
                }

                return CreateSyncResult(item, uploadResult);
            case TransferettoSyncAction.DownloadFile:
                TransferettoTransferResult downloadResult = DownloadSftpFile(session, item.RemotePath!, item.LocalPath!, transferOptions);
                if (syncOptions.PreserveTimestamps && item.Source?.LastWriteTimeUtc is DateTime downloadTimestamp && File.Exists(item.LocalPath)) {
                    File.SetLastWriteTimeUtc(item.LocalPath!, downloadTimestamp);
                }

                return CreateSyncResult(item, downloadResult);
            case TransferettoSyncAction.DeleteRemoteFile:
                RemoveSftpFile(session, item.RemotePath!);
                return CreateSyncResult(item, true, false, false, null, item.Message);
            case TransferettoSyncAction.DeleteRemoteDirectory:
                RemoveSftpDirectory(session, item.RemotePath!);
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

    private static IReadOnlyList<TransferettoSyncEntry> BuildSftpRemoteSyncManifest(
        TransferettoSftpSession session,
        string remoteRoot,
        string localRoot) {
        List<TransferettoSyncEntry> entries = new();
        BuildSftpRemoteSyncManifest(session, remoteRoot, remoteRoot, localRoot, entries);
        return entries;
    }

    private static void BuildSftpRemoteSyncManifest(
        TransferettoSftpSession session,
        string remoteRoot,
        string currentRemotePath,
        string localRoot,
        ICollection<TransferettoSyncEntry> entries) {
        foreach (TransferettoSftpItem item in GetSftpListing(session, currentRemotePath)) {
            if (IsSpecialRemoteDirectoryName(item.Name)) {
                continue;
            }

            string relativePath = GetRemoteRelativePath(remoteRoot, item.FullName);
            string localPath = Path.Combine(localRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!item.IsDirectory && !item.IsRegularFile) {
                continue;
            }

            entries.Add(new TransferettoSyncEntry {
                RelativePath = relativePath,
                LocalPath = localPath,
                RemotePath = item.FullName,
                IsDirectory = item.IsDirectory,
                Length = item.IsRegularFile ? item.Length : null,
                LastWriteTimeUtc = ToUtcTimestamp(item.LastWriteTime)
            });

            if (item.IsDirectory) {
                BuildSftpRemoteSyncManifest(session, remoteRoot, item.FullName, localRoot, entries);
            }
        }
    }
}
