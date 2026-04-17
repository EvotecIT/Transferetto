using System;
using System.Collections.Generic;
using System.Linq;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto;
/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>

public static partial class TransferettoClient {
    /// <summary>
    /// Gets the current FTP or FTPS working directory.
    /// </summary>
    public static string GetFtpWorkingDirectory(TransferettoFtpSession session) {
        EnsureNotNull(session, nameof(session));
        return session.Client.GetWorkingDirectory();
    }
    /// <summary>
    /// Changes the current FTP or FTPS working directory.
    /// </summary>

    public static void SetFtpWorkingDirectory(TransferettoFtpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        session.Client.SetWorkingDirectory(path);
    }
    /// <summary>
    /// Detects a compatible FTP or FTPS configuration profile.
    /// </summary>

    public static IReadOnlyList<FtpProfile> DetectFtpConfiguration(TransferettoFtpConnectionOptions options, bool firstOnly) {
        EnsureNotNull(options, nameof(options));

        using FtpClient client = new(options.Server);
        if (options.Port.HasValue) {
            client.Port = options.Port.Value;
        }
        if (options.Credential is not null) {
            client.Credentials = options.Credential;
        }

        return client.AutoDetect(firstOnly);
    }
    /// <summary>
    /// Gets metadata for a single FTP or FTPS item.
    /// </summary>

    public static TransferettoRemoteItem? GetFtpItem(TransferettoFtpSession session, string path, bool followLinks = false) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        FtpListItem? item = session.Client.GetObjectInfo(path, followLinks);
        return item is null ? null : TransferettoRemoteItem.FromFtpListItem(item);
    }
    /// <summary>
    /// Gets the size of a remote FTP or FTPS file.
    /// </summary>

    public static long GetFtpFileSize(TransferettoFtpSession session, string remotePath, long defaultValue = -1) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.GetFileSize(remotePath, defaultValue);
    }
    /// <summary>
    /// Gets the last modified time of a remote FTP or FTPS item.
    /// </summary>

    public static DateTime GetFtpModifiedTime(TransferettoFtpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.GetModifiedTime(remotePath);
    }
    /// <summary>
    /// Updates the last modified time of a remote FTP or FTPS item.
    /// </summary>

    public static TransferettoOperationResult SetFtpModifiedTime(TransferettoFtpSession session, string remotePath, DateTime modifiedTime) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        session.Client.SetModifiedTime(remotePath, modifiedTime);
        return new TransferettoOperationResult {
            Action = "SetModifiedTime",
            Status = true,
            Path = remotePath,
            Message = modifiedTime.ToString("O")
        };
    }
    /// <summary>
    /// Compares a local file with a remote FTP or FTPS file.
    /// </summary>

    public static object CompareFtpFile(TransferettoFtpSession session, string localPath, string remotePath, FtpCompareOption compareOption = FtpCompareOption.Auto) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.CompareFile(localPath, remotePath, compareOption);
    }
    /// <summary>
    /// Gets the checksum reported by an FTP or FTPS server.
    /// </summary>

    public static object GetFtpChecksum(TransferettoFtpSession session, string remotePath, FtpHashAlgorithm hashAlgorithm = FtpHashAlgorithm.MD5) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.GetChecksum(remotePath, hashAlgorithm);
    }
    /// <summary>
    /// Gets permissions for a remote FTP or FTPS item.
    /// </summary>

    public static object GetFtpChmod(TransferettoFtpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.GetChmod(remotePath);
    }
    /// <summary>
    /// Sets permissions for a remote FTP or FTPS item.
    /// </summary>

    public static void SetFtpChmod(TransferettoFtpSession session, string remotePath, int permissions) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        session.Client.Chmod(remotePath, permissions);
    }
    /// <summary>
    /// Sets permissions for a remote FTP or FTPS item.
    /// </summary>

    public static void SetFtpChmod(TransferettoFtpSession session, string remotePath, FtpPermission owner, FtpPermission group, FtpPermission other) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        session.Client.Chmod(remotePath, owner, group, other);
    }
    /// <summary>
    /// Updates mutable FTP or FTPS session options.
    /// </summary>

    public static void SetFtpOption(TransferettoFtpSession session, int? retryAttempts, bool? downloadZeroByteFiles) {
        EnsureNotNull(session, nameof(session));

        if (retryAttempts.HasValue) {
            session.Client.Config.RetryAttempts = retryAttempts.Value;
        }

        if (downloadZeroByteFiles.HasValue) {
            session.Client.Config.DownloadZeroByteFiles = downloadZeroByteFiles.Value;
        }
    }
    /// <summary>
    /// Moves a remote FTP or FTPS file.
    /// </summary>

    public static void MoveFtpFile(TransferettoFtpSession session, string remoteSource, string remoteDestination, FtpRemoteExists remoteExists = FtpRemoteExists.Skip) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remoteSource, nameof(remoteSource));
        EnsureNotNullOrWhiteSpace(remoteDestination, nameof(remoteDestination));
        session.Client.MoveFile(remoteSource, remoteDestination, remoteExists);
    }
    /// <summary>
    /// Moves a remote FTP or FTPS directory.
    /// </summary>

    public static void MoveFtpDirectory(TransferettoFtpSession session, string remoteSource, string remoteDestination, FtpRemoteExists remoteExists = FtpRemoteExists.Skip) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remoteSource, nameof(remoteSource));
        EnsureNotNullOrWhiteSpace(remoteDestination, nameof(remoteDestination));
        session.Client.MoveDirectory(remoteSource, remoteDestination, remoteExists);
    }
    /// <summary>
    /// Removes a remote FTP or FTPS file.
    /// </summary>

    public static void RemoveFtpFile(TransferettoFtpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        session.Client.DeleteFile(remotePath);
    }
    /// <summary>
    /// Removes a remote FTP or FTPS directory.
    /// </summary>

    public static void RemoveFtpDirectory(TransferettoFtpSession session, string remotePath, FtpListOption? listOption = null) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        if (listOption.HasValue) {
            session.Client.DeleteDirectory(remotePath, listOption.Value);
        } else {
            session.Client.DeleteDirectory(remotePath);
        }
    }
    /// <summary>
    /// Creates a remote FTP or FTPS directory.
    /// </summary>

    public static TransferettoOperationResult CreateFtpDirectory(TransferettoFtpSession session, string remotePath, bool force = false) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        session.Client.CreateDirectory(remotePath, force);
        return new TransferettoOperationResult {
            Action = "CreateDirectory",
            Status = true,
            Path = remotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Renames a remote FTP or FTPS file.
    /// </summary>

    public static void RenameFtpFile(TransferettoFtpSession session, string sourcePath, string destinationPath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));
        session.Client.Rename(sourcePath, destinationPath);
    }
    /// <summary>
    /// Tests whether a remote FTP or FTPS file exists.
    /// </summary>

    public static bool TestFtpFile(TransferettoFtpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.FileExists(remotePath);
    }
    /// <summary>
    /// Tests whether a remote FTP or FTPS directory exists.
    /// </summary>

    public static bool TestFtpDirectory(TransferettoFtpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        return session.Client.DirectoryExists(remotePath);
    }
    /// <summary>
    /// Uploads a local directory to an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadFtpDirectory(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        IEnumerable<FtpRule>? rules = null) {
        return UploadFtpDirectory(session, localPath, remotePath, folderSyncMode, remoteExists, verifyOptions, rules, null);
    }
    /// <summary>
    /// Uploads a local directory to an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadFtpDirectory(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode,
        FtpRemoteExists remoteExists,
        FtpVerify verifyOptions,
        IEnumerable<FtpRule>? rules,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "UploadDirectory",
                    TransferettoTransferDirection.Upload,
                    localPath,
                    remotePath,
                    ftpProgress,
                    null,
                    lastReportedBytes);
            };

        List<FtpResult> results = session.Client.UploadDirectory(localPath, remotePath, folderSyncMode, remoteExists, verifyOptions, rules?.ToList(), progress);

        return results.Select(result => FromFtpResult("UploadDirectory", localPath, result)).ToArray();
    }
    /// <summary>
    /// Downloads a remote directory from an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpDirectory(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update) {
        return DownloadFtpDirectory(session, localPath, remotePath, folderSyncMode, FtpLocalExists.Skip, FtpVerify.None, null, null);
    }
    /// <summary>
    /// Downloads a remote directory from an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpDirectory(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode,
        FtpLocalExists localExists,
        FtpVerify verifyOptions,
        IEnumerable<FtpRule>? rules,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "DownloadDirectory",
                    TransferettoTransferDirection.Download,
                    localPath,
                    remotePath,
                    ftpProgress,
                    null,
                    lastReportedBytes);
            };

        List<FtpResult> results = session.Client.DownloadDirectory(localPath, remotePath, folderSyncMode, localExists, verifyOptions, rules?.ToList(), progress);
        return results.Select(result => FromFtpResult("DownloadDirectory", localPath, result)).ToArray();
    }
    /// <summary>
    /// Tests whether an FXP transfer is ready to run.
    /// </summary>

    public static TransferettoFxpPreflightResult TestFxpTransfer(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        TransferettoFxpTransferKind transferKind = TransferettoFxpTransferKind.File,
        bool createRemoteDirectory = false) {
        EnsureNotNull(sourceSession, nameof(sourceSession));
        EnsureNotNull(destinationSession, nameof(destinationSession));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        List<string> messages = new();
        bool sourceConnected = sourceSession.Client.IsConnected;
        bool destinationConnected = destinationSession.Client.IsConnected;
        if (!sourceConnected) {
            messages.Add("Source FTP session is not connected.");
        }

        if (!destinationConnected) {
            messages.Add("Destination FTP session is not connected.");
        }

        bool sourcePathExists = sourceConnected && TestFxpSourcePath(sourceSession, sourcePath, transferKind, messages);
        bool destinationParentExists = destinationConnected && TestFxpDestinationParent(destinationSession, destinationPath, createRemoteDirectory, messages);
        bool isSuccess = sourceConnected && destinationConnected && sourcePathExists && destinationParentExists;
        AddFxpCapabilityMessages(sourceSession, destinationSession, messages);

        return new TransferettoFxpPreflightResult {
            Status = isSuccess,
            IsSuccess = isSuccess,
            TransferKind = transferKind,
            SourceHost = sourceSession.Host,
            SourcePort = sourceSession.Port,
            DestinationHost = destinationSession.Host,
            DestinationPort = destinationSession.Port,
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            SourceConnected = sourceConnected,
            DestinationConnected = destinationConnected,
            SourcePathExists = sourcePathExists,
            DestinationParentExists = destinationParentExists,
            SourceSupportsCpsv = SourceSupportsFxpCapability(sourceSession, FtpCapability.CPSV),
            DestinationSupportsCpsv = SourceSupportsFxpCapability(destinationSession, FtpCapability.CPSV),
            SourceSupportsSscn = SourceSupportsFxpCapability(sourceSession, FtpCapability.SSCN),
            DestinationSupportsSscn = SourceSupportsFxpCapability(destinationSession, FtpCapability.SSCN),
            SourceSupportsEpsv = SourceSupportsFxpCapability(sourceSession, FtpCapability.EPSV),
            DestinationSupportsEpsv = SourceSupportsFxpCapability(destinationSession, FtpCapability.EPSV),
            Messages = messages.ToArray()
        };
    }
    /// <summary>
    /// Transfers a file directly between two FTP or FTPS sessions.
    /// </summary>

    public static TransferettoTransferResult StartFxpFileTransfer(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        bool createRemoteDirectory = false,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None) {
        return StartFxpFileTransfer(sourceSession, sourcePath, destinationSession, destinationPath, createRemoteDirectory, remoteExists, verifyOptions, null);
    }
    /// <summary>
    /// Transfers a file directly between two FTP or FTPS sessions.
    /// </summary>

    public static TransferettoTransferResult StartFxpFileTransfer(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        bool createRemoteDirectory,
        FtpRemoteExists remoteExists,
        FtpVerify verifyOptions,
        TransferettoTransferOptions? options) {
        EnsureNotNull(sourceSession, nameof(sourceSession));
        EnsureNotNull(destinationSession, nameof(destinationSession));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        long sourceSize = GetFtpFileSize(sourceSession, sourcePath, -1);
        long? totalBytes = sourceSize >= 0 ? sourceSize : null;
        long bytesTransferred = 0;
        DateTime startedUtc = DateTime.UtcNow;
        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                bytesTransferred = Math.Max(bytesTransferred, NormalizeTransferredBytes(ftpProgress.TransferredBytes));
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "TransferFile",
                    TransferettoTransferDirection.Transfer,
                    sourcePath,
                    destinationPath,
                    ftpProgress,
                    totalBytes,
                    lastReportedBytes,
                    protocol: "FXP");
            };

        FtpStatus status = sourceSession.Client.TransferFile(
            sourcePath,
            destinationSession.Client,
            destinationPath,
            createRemoteDirectory,
            remoteExists,
            verifyOptions,
            progress,
            null!);

        if (status == FtpStatus.Success && totalBytes.HasValue) {
            bytesTransferred = Math.Max(bytesTransferred, totalBytes.Value);
            lastReportedBytes = ReportFtpTransferProgress(
                options,
                "TransferFile",
                TransferettoTransferDirection.Transfer,
                sourcePath,
                destinationPath,
                bytesTransferred,
                totalBytes,
                lastReportedBytes,
                protocol: "FXP",
                force: true);
        }

        DateTime completedUtc = DateTime.UtcNow;

        return new TransferettoTransferResult {
            Action = "TransferFile",
            Status = status is FtpStatus.Success or FtpStatus.Skipped,
            IsSuccess = status is FtpStatus.Success or FtpStatus.Skipped,
            IsSkipped = status == FtpStatus.Skipped,
            IsSkippedByRule = false,
            IsFailed = status == FtpStatus.Failed,
            LocalPath = sourcePath,
            RemotePath = destinationPath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = status.ToString()
        };
    }
    /// <summary>
    /// Transfers a directory directly between two FTP or FTPS sessions.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> StartFxpDirectoryTransfer(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None) {
        return StartFxpDirectoryTransfer(sourceSession, sourcePath, destinationSession, destinationPath, folderSyncMode, remoteExists, verifyOptions, null, null);
    }
    /// <summary>
    /// Transfers a directory directly between two FTP or FTPS sessions.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> StartFxpDirectoryTransfer(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        FtpFolderSyncMode folderSyncMode,
        FtpRemoteExists remoteExists,
        FtpVerify verifyOptions,
        IEnumerable<FtpRule>? rules,
        TransferettoTransferOptions? options) {
        EnsureNotNull(sourceSession, nameof(sourceSession));
        EnsureNotNull(destinationSession, nameof(destinationSession));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "TransferDirectory",
                    TransferettoTransferDirection.Transfer,
                    sourcePath,
                    destinationPath,
                    ftpProgress,
                    null,
                    lastReportedBytes,
                    protocol: "FXP");
            };

        List<FtpResult> results = sourceSession.Client.TransferDirectory(
            sourcePath,
            destinationSession.Client,
            destinationPath,
            folderSyncMode,
            remoteExists,
            verifyOptions,
            rules?.ToList(),
            progress);

        return results.Select(result => FromFtpResult("TransferDirectory", sourcePath, result)).ToArray();
    }

    private static TransferettoTransferResult FromFtpResult(string action, string? localPath, FtpResult result) {
        return new TransferettoTransferResult {
            Action = action,
            Status = result.IsSuccess,
            IsSuccess = result.IsSuccess,
            IsSkipped = result.IsSkipped,
            IsSkippedByRule = result.IsSkippedByRule,
            IsFailed = result.IsFailed,
            LocalPath = result.LocalPath ?? localPath,
            RemotePath = result.RemotePath,
            BytesTransferred = result.Size >= 0 && result.IsSuccess && !result.IsSkipped ? result.Size : null,
            TotalBytes = result.Size >= 0 ? result.Size : null,
            Message = result.Exception?.Message ?? (result.IsSkipped ? "Skipped" : "Success")
        };
    }

    private static bool TestFxpSourcePath(
        TransferettoFtpSession session,
        string sourcePath,
        TransferettoFxpTransferKind transferKind,
        ICollection<string> messages) {
        try {
            bool exists = transferKind == TransferettoFxpTransferKind.Directory
                ? session.Client.DirectoryExists(sourcePath)
                : session.Client.FileExists(sourcePath);
            if (!exists) {
                messages.Add($"Source {transferKind.ToString().ToLowerInvariant()} path does not exist: {sourcePath}");
            }

            return exists;
        } catch (Exception exception) {
            messages.Add($"Could not test source path {sourcePath}: {exception.Message}");
            return false;
        }
    }

    private static bool TestFxpDestinationParent(
        TransferettoFtpSession session,
        string destinationPath,
        bool createRemoteDirectory,
        ICollection<string> messages) {
        string parentPath = GetRemoteParent(destinationPath);
        if (string.IsNullOrWhiteSpace(parentPath) || parentPath == "/" || parentPath == ".") {
            return true;
        }

        try {
            bool exists = session.Client.DirectoryExists(parentPath);
            if (!exists && createRemoteDirectory) {
                messages.Add($"Destination parent does not exist but create-remote-directory was requested: {parentPath}");
                return true;
            }

            if (!exists) {
                messages.Add($"Destination parent directory does not exist: {parentPath}");
            }

            return exists;
        } catch (Exception exception) {
            messages.Add($"Could not test destination parent {parentPath}: {exception.Message}");
            return false;
        }
    }

    private static void AddFxpCapabilityMessages(
        TransferettoFtpSession sourceSession,
        TransferettoFtpSession destinationSession,
        ICollection<string> messages) {
        bool sourceAdvertisesFxp = SourceSupportsFxpCapability(sourceSession, FtpCapability.CPSV) || SourceSupportsFxpCapability(sourceSession, FtpCapability.SSCN);
        bool destinationAdvertisesFxp = SourceSupportsFxpCapability(destinationSession, FtpCapability.CPSV) || SourceSupportsFxpCapability(destinationSession, FtpCapability.SSCN);
        if (!sourceAdvertisesFxp) {
            messages.Add("Source server did not advertise CPSV or SSCN. FXP may still work, but server policy must allow passive/active negotiation.");
        }

        if (!destinationAdvertisesFxp) {
            messages.Add("Destination server did not advertise CPSV or SSCN. FXP may still work, but server policy must allow passive/active negotiation.");
        }
    }

    private static bool SourceSupportsFxpCapability(TransferettoFtpSession session, FtpCapability capability) {
        return session.Client.IsConnected && session.Client.HasFeature(capability);
    }
}
