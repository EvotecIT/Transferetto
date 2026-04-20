using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Transferetto;
/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>

public static partial class TransferettoClient {
    /// <summary>
    /// Creates and connects an SCP session.
    /// </summary>
    public static TransferettoScpSession ConnectScp(TransferettoSshConnectionOptions options) {
        EnsureNotNull(options, nameof(options));
        EnsureNotNullOrWhiteSpace(options.Server, nameof(options.Server));
        ValidateSshHostKeyTrustOptions(options);

        ScpClient client = CreateScpClient(options);
        TransferettoSshHostKeyInfo? hostKeyInfo = null;
        client.HostKeyReceived += (_, args) => {
            hostKeyInfo = EvaluateHostKeyTrust(options, args);
            args.CanTrust = hostKeyInfo.CanTrust;
        };

        try {
            ApplyScpClientOptions(client, options);
            client.Connect();
            return new TransferettoScpSession(client) {
                HostKeyInfo = hostKeyInfo
            };
        } catch {
            client.Dispose();
            throw;
        }
    }
    /// <summary>
    /// Closes an SCP session.
    /// </summary>

    public static void DisconnectScp(TransferettoScpSession session) {
        EnsureNotNull(session, nameof(session));
        if (session.Client.IsConnected) {
            session.Client.Disconnect();
        }
    }
    /// <summary>
    /// Uploads a file over SCP.
    /// </summary>

    public static TransferettoTransferResult UploadScpFile(TransferettoScpSession session, string localPath, string remotePath) {
        return UploadScpFile(session, localPath, remotePath, null);
    }
    /// <summary>
    /// Uploads a file over SCP.
    /// </summary>

    public static TransferettoTransferResult UploadScpFile(TransferettoScpSession session, string localPath, string remotePath, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureFileExists(localPath, nameof(localPath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        FileInfo fileInfo = new(localPath);
        long totalBytes = fileInfo.Length;
        long bytesTransferred = 0;
        long lastReportedBytes = 0;
        DateTime startedUtc = DateTime.UtcNow;
        EventHandler<ScpUploadEventArgs>? progress = options is null
            ? null
            : (_, args) => {
                bytesTransferred = Math.Max(bytesTransferred, NormalizeTransferredBytes(args.Uploaded));
                lastReportedBytes = ReportScpTransferProgress(
                    options,
                    "UploadFile",
                    TransferettoTransferDirection.Upload,
                    localPath,
                    remotePath,
                    bytesTransferred,
                    args.Size,
                    lastReportedBytes);
            };

        try {
            if (progress is not null) {
                session.Client.Uploading += progress;
            }

            session.Client.Upload(fileInfo, remotePath);
        } finally {
            if (progress is not null) {
                session.Client.Uploading -= progress;
            }
        }

        bytesTransferred = Math.Max(bytesTransferred, totalBytes);
        lastReportedBytes = ReportScpTransferProgress(
            options,
            "UploadFile",
            TransferettoTransferDirection.Upload,
            localPath,
            remotePath,
            bytesTransferred,
            totalBytes,
            lastReportedBytes,
            force: true);
        DateTime completedUtc = DateTime.UtcNow;
        return new TransferettoTransferResult {
            Action = "UploadFile",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Downloads a file over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpFile(TransferettoScpSession session, string remotePath, string localPath) {
        return DownloadScpFile(session, remotePath, localPath, null);
    }
    /// <summary>
    /// Downloads a file over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpFile(TransferettoScpSession session, string remotePath, string localPath, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        FileInfo fileInfo = new(localPath);
        long bytesTransferred = 0;
        long? totalBytes = null;
        long lastReportedBytes = 0;
        DateTime startedUtc = DateTime.UtcNow;
        EventHandler<ScpDownloadEventArgs>? progress = options is null
            ? null
            : (_, args) => {
                bytesTransferred = Math.Max(bytesTransferred, NormalizeTransferredBytes(args.Downloaded));
                totalBytes = args.Size;
                lastReportedBytes = ReportScpTransferProgress(
                    options,
                    "DownloadFile",
                    TransferettoTransferDirection.Download,
                    localPath,
                    remotePath,
                    bytesTransferred,
                    totalBytes,
                    lastReportedBytes);
            };

        try {
            if (progress is not null) {
                session.Client.Downloading += progress;
            }

            session.Client.Download(remotePath, fileInfo);
        } finally {
            if (progress is not null) {
                session.Client.Downloading -= progress;
            }
        }

        if (fileInfo.Exists) {
            fileInfo.Refresh();
            bytesTransferred = Math.Max(bytesTransferred, fileInfo.Length);
        }

        lastReportedBytes = ReportScpTransferProgress(
            options,
            "DownloadFile",
            TransferettoTransferDirection.Download,
            localPath,
            remotePath,
            bytesTransferred,
            totalBytes,
            lastReportedBytes,
            force: true);
        DateTime completedUtc = DateTime.UtcNow;
        return new TransferettoTransferResult {
            Action = "DownloadFile",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Uploads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult UploadScpDirectory(TransferettoScpSession session, string localPath, string remotePath) {
        return UploadScpDirectory(session, localPath, remotePath, null);
    }
    /// <summary>
    /// Uploads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult UploadScpDirectory(TransferettoScpSession session, string localPath, string remotePath, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        DirectoryInfo directoryInfo = new(localPath);
        if (!directoryInfo.Exists) {
            throw new DirectoryNotFoundException($"Directory {localPath} does not exist.");
        }

        Dictionary<string, long> transferredByFile = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, long> totalByFile = new(StringComparer.OrdinalIgnoreCase);
        long bytesTransferred = 0;
        long totalBytes = 0;
        long lastReportedBytes = 0;
        DateTime startedUtc = DateTime.UtcNow;
        EventHandler<ScpUploadEventArgs>? progress = options is null
            ? null
            : (_, args) => {
                string key = args.Filename ?? string.Empty;
                transferredByFile[key] = NormalizeTransferredBytes(args.Uploaded);
                totalByFile[key] = NormalizeTransferredBytes(args.Size);
                bytesTransferred = SumTransferredBytes(transferredByFile);
                totalBytes = SumTransferredBytes(totalByFile);
                lastReportedBytes = ReportScpTransferProgress(
                    options,
                    "UploadDirectory",
                    TransferettoTransferDirection.Upload,
                    localPath,
                    args.Filename ?? remotePath,
                    bytesTransferred,
                    totalBytes,
                    lastReportedBytes);
            };

        try {
            if (progress is not null) {
                session.Client.Uploading += progress;
            }

            session.Client.Upload(directoryInfo, remotePath);
        } finally {
            if (progress is not null) {
                session.Client.Uploading -= progress;
            }
        }

        lastReportedBytes = ReportScpTransferProgress(
            options,
            "UploadDirectory",
            TransferettoTransferDirection.Upload,
            localPath,
            remotePath,
            bytesTransferred,
            totalBytes > 0 ? totalBytes : null,
            lastReportedBytes,
            force: true);
        DateTime completedUtc = DateTime.UtcNow;
        return new TransferettoTransferResult {
            Action = "UploadDirectory",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes > 0 ? totalBytes : null,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Downloads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpDirectory(TransferettoScpSession session, string remotePath, string localPath) {
        return DownloadScpDirectory(session, remotePath, localPath, null);
    }
    /// <summary>
    /// Downloads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpDirectory(TransferettoScpSession session, string remotePath, string localPath, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        DirectoryInfo directoryInfo = new(localPath);
        if (!directoryInfo.Exists) {
            directoryInfo.Create();
        }

        Dictionary<string, long> transferredByFile = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, long> totalByFile = new(StringComparer.OrdinalIgnoreCase);
        long bytesTransferred = 0;
        long totalBytes = 0;
        long lastReportedBytes = 0;
        DateTime startedUtc = DateTime.UtcNow;
        EventHandler<ScpDownloadEventArgs>? progress = options is null
            ? null
            : (_, args) => {
                string key = args.Filename ?? string.Empty;
                transferredByFile[key] = NormalizeTransferredBytes(args.Downloaded);
                totalByFile[key] = NormalizeTransferredBytes(args.Size);
                bytesTransferred = SumTransferredBytes(transferredByFile);
                totalBytes = SumTransferredBytes(totalByFile);
                lastReportedBytes = ReportScpTransferProgress(
                    options,
                    "DownloadDirectory",
                    TransferettoTransferDirection.Download,
                    localPath,
                    args.Filename ?? remotePath,
                    bytesTransferred,
                    totalBytes,
                    lastReportedBytes);
            };

        try {
            if (progress is not null) {
                session.Client.Downloading += progress;
            }

            session.Client.Download(remotePath, directoryInfo);
        } finally {
            if (progress is not null) {
                session.Client.Downloading -= progress;
            }
        }

        lastReportedBytes = ReportScpTransferProgress(
            options,
            "DownloadDirectory",
            TransferettoTransferDirection.Download,
            localPath,
            remotePath,
            bytesTransferred,
            totalBytes > 0 ? totalBytes : null,
            lastReportedBytes,
            force: true);
        DateTime completedUtc = DateTime.UtcNow;
        return new TransferettoTransferResult {
            Action = "DownloadDirectory",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes > 0 ? totalBytes : null,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = string.Empty
        };
    }

    private static ScpClient CreateScpClient(TransferettoSshConnectionOptions options) {
        return new ScpClient(CreateSshConnectionInfo(options));
    }

    private static void ApplyScpClientOptions(ScpClient client, TransferettoSshConnectionOptions options) {
        if (options.KeepAliveIntervalSeconds.HasValue) {
            client.KeepAliveInterval = TimeSpan.FromSeconds(options.KeepAliveIntervalSeconds.Value);
        }

        if (options.ConnectionTimeoutSeconds.HasValue) {
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds.Value);
        }

        if (options.RetryAttempts.HasValue) {
            client.ConnectionInfo.RetryAttempts = options.RetryAttempts.Value;
        }
    }

    private static long ReportScpTransferProgress(
        TransferettoTransferOptions? options,
        string action,
        TransferettoTransferDirection direction,
        string? localPath,
        string? remotePath,
        long transferredBytes,
        long? totalBytes,
        long lastReportedBytes,
        bool force = false) {
        if (options is null) {
            return lastReportedBytes;
        }

        long normalizedTransferredBytes = NormalizeTransferredBytes(transferredBytes);
        return ReportTransferProgress(
            options,
            action,
            "SCP",
            direction,
            localPath,
            remotePath,
            (ulong) normalizedTransferredBytes,
            totalBytes,
            lastReportedBytes,
            force);
    }

    private static long SumTransferredBytes(Dictionary<string, long> values) {
        long total = 0;
        foreach (long value in values.Values) {
            total += value;
        }

        return total;
    }
}
