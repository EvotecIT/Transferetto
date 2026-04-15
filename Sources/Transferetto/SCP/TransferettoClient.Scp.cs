using System;
using System.IO;
using Renci.SshNet;

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
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureFileExists(localPath, nameof(localPath));

        FileInfo fileInfo = new(localPath);
        session.Client.Upload(fileInfo, remotePath);
        return new TransferettoTransferResult {
            Action = "UploadFile",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Downloads a file over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpFile(TransferettoScpSession session, string remotePath, string localPath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));

        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        FileInfo fileInfo = new(localPath);
        session.Client.Download(remotePath, fileInfo);
        return new TransferettoTransferResult {
            Action = "DownloadFile",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Uploads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult UploadScpDirectory(TransferettoScpSession session, string localPath, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        DirectoryInfo directoryInfo = new(localPath);
        if (!directoryInfo.Exists) {
            throw new DirectoryNotFoundException($"Directory {localPath} does not exist.");
        }

        session.Client.Upload(directoryInfo, remotePath);
        return new TransferettoTransferResult {
            Action = "UploadDirectory",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Downloads a directory over SCP.
    /// </summary>

    public static TransferettoTransferResult DownloadScpDirectory(TransferettoScpSession session, string remotePath, string localPath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));

        DirectoryInfo directoryInfo = new(localPath);
        if (!directoryInfo.Exists) {
            directoryInfo.Create();
        }

        session.Client.Download(remotePath, directoryInfo);
        return new TransferettoTransferResult {
            Action = "DownloadDirectory",
            Status = true,
            IsSuccess = true,
            IsSkipped = false,
            IsSkippedByRule = false,
            IsFailed = false,
            LocalPath = localPath,
            RemotePath = remotePath,
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
}
