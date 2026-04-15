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
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        List<FtpResult> results = rules is not null
            ? session.Client.UploadDirectory(localPath, remotePath, folderSyncMode, remoteExists, verifyOptions, rules.ToList())
            : session.Client.UploadDirectory(localPath, remotePath, folderSyncMode, remoteExists, verifyOptions);

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
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        List<FtpResult> results = session.Client.DownloadDirectory(localPath, remotePath, folderSyncMode);
        return results.Select(result => FromFtpResult("DownloadDirectory", localPath, result)).ToArray();
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
        EnsureNotNull(sourceSession, nameof(sourceSession));
        EnsureNotNull(destinationSession, nameof(destinationSession));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        FtpStatus status = sourceSession.Client.TransferFile(
            sourcePath,
            destinationSession.Client,
            destinationPath,
            createRemoteDirectory,
            remoteExists,
            verifyOptions);

        return new TransferettoTransferResult {
            Action = "TransferFile",
            Status = status is FtpStatus.Success or FtpStatus.Skipped,
            IsSuccess = status is FtpStatus.Success or FtpStatus.Skipped,
            IsSkipped = status == FtpStatus.Skipped,
            IsSkippedByRule = false,
            IsFailed = status == FtpStatus.Failed,
            LocalPath = sourcePath,
            RemotePath = destinationPath,
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
        EnsureNotNull(sourceSession, nameof(sourceSession));
        EnsureNotNull(destinationSession, nameof(destinationSession));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        List<FtpResult> results = sourceSession.Client.TransferDirectory(
            sourcePath,
            destinationSession.Client,
            destinationPath,
            folderSyncMode,
            remoteExists,
            verifyOptions);

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
            Message = result.Exception?.Message ?? (result.IsSkipped ? "Skipped" : "Success")
        };
    }
}
