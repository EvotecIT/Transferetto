using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Transferetto;
/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>

public static partial class TransferettoClient {
    /// <summary>
    /// Creates and connects an SFTP session.
    /// </summary>
    public static TransferettoSftpSession ConnectSftp(TransferettoSftpConnectionOptions options) {
        EnsureNotNull(options, nameof(options));
        EnsureNotNullOrWhiteSpace(options.Server, nameof(options.Server));
        ValidateSshHostKeyTrustOptions(options);

        SftpClient client = CreateSftpClient(options);
        TransferettoSshHostKeyInfo? hostKeyInfo = null;
        client.HostKeyReceived += (_, args) => {
            hostKeyInfo = EvaluateHostKeyTrust(options, args);
            args.CanTrust = hostKeyInfo.CanTrust;
        };

        try {
            ApplySftpClientOptions(client, options);
            client.Connect();
            return new TransferettoSftpSession(client) {
                HostKeyInfo = hostKeyInfo
            };
        } catch {
            client.Dispose();
            throw;
        }
    }
    /// <summary>
    /// Closes an SFTP session.
    /// </summary>

    public static void DisconnectSftp(TransferettoSftpSession session) {
        EnsureNotNull(session, nameof(session));
        if (session.Client.IsConnected) {
            session.Client.Disconnect();
        }
    }
    /// <summary>
    /// Gets a remote SFTP directory listing.
    /// </summary>

    public static IReadOnlyList<TransferettoSftpItem> GetSftpListing(TransferettoSftpSession session, string? path = null) {
        EnsureNotNull(session, nameof(session));
        string listPath = path ?? string.Empty;
        return session.Client.ListDirectory(listPath).Select(static item => TransferettoSftpItem.FromSftpFile(item)).ToArray();
    }
    /// <summary>
    /// Gets the current SFTP working directory.
    /// </summary>

    public static string GetSftpWorkingDirectory(TransferettoSftpSession session) {
        EnsureNotNull(session, nameof(session));
        return session.Client.WorkingDirectory;
    }
    /// <summary>
    /// Changes the current SFTP working directory.
    /// </summary>

    public static void SetSftpWorkingDirectory(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        session.Client.ChangeDirectory(path);
    }
    /// <summary>
    /// Gets SFTP metadata for a remote item.
    /// </summary>

    public static TransferettoSftpAttributes GetSftpAttributes(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        return TransferettoSftpAttributes.FromFileAttributes(path, session.Client.GetAttributes(path));
    }
    /// <summary>
    /// Gets SFTP permission bits for a remote item.
    /// </summary>

    public static string GetSftpChmod(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return GetSftpAttributes(session, path).PermissionsOctal;
    }
    /// <summary>
    /// Sets SFTP permission bits for a remote item.
    /// </summary>

    public static TransferettoOperationResult SetSftpChmod(TransferettoSftpSession session, string path, string permissions) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        EnsureNotNullOrWhiteSpace(permissions, nameof(permissions));

        short mode = ParseSftpPermissions(permissions);
        session.Client.ChangePermissions(path, mode);
        return new TransferettoOperationResult {
            Action = "SetPermissions",
            Status = true,
            Path = path,
            Message = Convert.ToString(mode, 8).PadLeft(3, '0')
        };
    }
    /// <summary>
    /// Sets SFTP permission bits for a remote item.
    /// </summary>

    public static TransferettoOperationResult SetSftpChmod(TransferettoSftpSession session, string path, int owner, int group, int other) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        ValidatePermissionDigit(owner, nameof(owner));
        ValidatePermissionDigit(group, nameof(group));
        ValidatePermissionDigit(other, nameof(other));

        return SetSftpChmod(session, path, $"{owner}{group}{other}");
    }
    /// <summary>
    /// Updates timestamps on a remote SFTP item.
    /// </summary>

    public static TransferettoOperationResult SetSftpTimestamp(
        TransferettoSftpSession session,
        string path,
        DateTime? lastAccessTime,
        DateTime? lastWriteTime,
        bool useUtc = false) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        if (!lastAccessTime.HasValue && !lastWriteTime.HasValue) {
            throw new InvalidOperationException("At least one timestamp must be provided.");
        }

        if (lastAccessTime.HasValue) {
            DateTime accessValue = useUtc ? DateTime.SpecifyKind(lastAccessTime.Value, DateTimeKind.Utc) : lastAccessTime.Value.ToUniversalTime();
            session.Client.SetLastAccessTimeUtc(path, accessValue);
        }

        if (lastWriteTime.HasValue) {
            DateTime writeValue = useUtc ? DateTime.SpecifyKind(lastWriteTime.Value, DateTimeKind.Utc) : lastWriteTime.Value.ToUniversalTime();
            session.Client.SetLastWriteTimeUtc(path, writeValue);
        }

        return new TransferettoOperationResult {
            Action = "SetTimestamp",
            Status = true,
            Path = path,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Tests whether a remote SFTP path exists.
    /// </summary>

    public static bool TestSftpPath(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return session.Client.Exists(path);
    }
    /// <summary>
    /// Tests whether a remote SFTP file exists.
    /// </summary>

    public static bool TestSftpFile(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return session.Client.Exists(path) && !session.Client.GetAttributes(path).IsDirectory;
    }
    /// <summary>
    /// Tests whether a remote SFTP directory exists.
    /// </summary>

    public static bool TestSftpDirectory(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return session.Client.Exists(path) && session.Client.GetAttributes(path).IsDirectory;
    }
    /// <summary>
    /// Tests whether a remote SFTP path is a symbolic link.
    /// </summary>

    public static bool TestSftpSymbolicLink(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return session.Client.Exists(path) && session.Client.GetAttributes(path).IsSymbolicLink;
    }
    /// <summary>
    /// Creates a remote SFTP directory.
    /// </summary>

    public static TransferettoOperationResult CreateSftpDirectory(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        session.Client.CreateDirectory(path);
        return new TransferettoOperationResult {
            Action = "CreateDirectory",
            Status = true,
            Path = path,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Creates a remote SFTP symbolic link.
    /// </summary>

    public static TransferettoOperationResult CreateSftpSymbolicLink(TransferettoSftpSession session, string targetPath, string linkPath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(targetPath, nameof(targetPath));
        EnsureNotNullOrWhiteSpace(linkPath, nameof(linkPath));

        session.Client.SymbolicLink(targetPath, linkPath);
        return new TransferettoOperationResult {
            Action = "CreateSymbolicLink",
            Status = true,
            OldPath = targetPath,
            NewPath = linkPath,
            Path = linkPath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Removes a remote SFTP directory.
    /// </summary>

    public static TransferettoOperationResult RemoveSftpDirectory(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        session.Client.DeleteDirectory(path);
        return new TransferettoOperationResult {
            Action = "RemoveDirectory",
            Status = true,
            Path = path,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Uploads a file over SFTP.
    /// </summary>

    public static TransferettoTransferResult UploadSftpFile(TransferettoSftpSession session, string localPath, string remotePath, bool allowOverride) {
        return UploadSftpFile(session, localPath, remotePath, allowOverride, null);
    }
    /// <summary>
    /// Uploads a file over SFTP.
    /// </summary>

    public static TransferettoTransferResult UploadSftpFile(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        bool allowOverride,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        FileInfo fileInfo = new(localPath);
        long totalBytes = fileInfo.Length;
        DateTime startedUtc = DateTime.UtcNow;
        long bytesTransferred = 0;
        using FileStream fileStream = new(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        session.Client.UploadFile(fileStream, remotePath, allowOverride, transferredBytes => {
            bytesTransferred = ReportTransferProgress(
                resolvedOptions,
                "UploadFile",
                "SFTP",
                TransferettoTransferDirection.Upload,
                localPath,
                remotePath,
                transferredBytes,
                totalBytes,
                bytesTransferred);
        });
        if (bytesTransferred < totalBytes) {
            bytesTransferred = ReportTransferProgress(
                resolvedOptions,
                "UploadFile",
                "SFTP",
                TransferettoTransferDirection.Upload,
                localPath,
                remotePath,
                (ulong) totalBytes,
                totalBytes,
                bytesTransferred,
                force: true);
        }

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
    /// Downloads a file over SFTP.
    /// </summary>

    public static TransferettoTransferResult DownloadSftpFile(TransferettoSftpSession session, string remotePath, string localPath) {
        return DownloadSftpFile(session, remotePath, localPath, null);
    }
    /// <summary>
    /// Downloads a file over SFTP.
    /// </summary>

    public static TransferettoTransferResult DownloadSftpFile(
        TransferettoSftpSession session,
        string remotePath,
        string localPath,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        long totalBytes = session.Client.GetAttributes(remotePath).Size;
        DateTime startedUtc = DateTime.UtcNow;
        long bytesTransferred = 0;
        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        WriteLocalFileAtomically(localPath, fileStream => {
            session.Client.DownloadFile(remotePath, fileStream, transferredBytes => {
                bytesTransferred = ReportTransferProgress(
                    resolvedOptions,
                    "DownloadFile",
                    "SFTP",
                    TransferettoTransferDirection.Download,
                    localPath,
                    remotePath,
                    transferredBytes,
                    totalBytes,
                    bytesTransferred);
            });
        });
        if (bytesTransferred < totalBytes) {
            bytesTransferred = ReportTransferProgress(
                resolvedOptions,
                "DownloadFile",
                "SFTP",
                TransferettoTransferDirection.Download,
                localPath,
                remotePath,
                (ulong) totalBytes,
                totalBytes,
                bytesTransferred,
                force: true);
        }

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
    /// Uploads a directory over SFTP.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadSftpDirectory(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        bool allowOverride = false) {
        return UploadSftpDirectory(session, localPath, remotePath, allowOverride, null);
    }
    /// <summary>
    /// Uploads a directory over SFTP.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadSftpDirectory(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        bool allowOverride,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(localPath)) {
            throw new DirectoryNotFoundException($"Directory {localPath} does not exist.");
        }

        List<TransferettoTransferResult> results = new();
        string normalizedRemotePath = NormalizeRemotePath(remotePath);
        EnsureSftpDirectoryExists(session, normalizedRemotePath, results, resolvedOptions);
        UploadSftpDirectoryInternal(session, new DirectoryInfo(localPath), normalizedRemotePath, allowOverride, resolvedOptions, results);
        return results;
    }
    /// <summary>
    /// Downloads a directory over SFTP.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadSftpDirectory(
        TransferettoSftpSession session,
        string remotePath,
        string localPath,
        bool allowOverride = false) {
        return DownloadSftpDirectory(session, remotePath, localPath, allowOverride, null);
    }
    /// <summary>
    /// Downloads a directory over SFTP.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadSftpDirectory(
        TransferettoSftpSession session,
        string remotePath,
        string localPath,
        bool allowOverride,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        string normalizedRemotePath = NormalizeRemotePath(remotePath);
        if (!session.Client.Exists(normalizedRemotePath) || !session.Client.GetAttributes(normalizedRemotePath).IsDirectory) {
            throw new DirectoryNotFoundException($"Remote directory {normalizedRemotePath} does not exist.");
        }

        List<TransferettoTransferResult> results = new();
        Directory.CreateDirectory(localPath);
        results.Add(CreateDirectoryTransferResult(localPath, normalizedRemotePath));
        DownloadSftpDirectoryInternal(session, normalizedRemotePath, localPath, allowOverride, resolvedOptions, results);
        return results;
    }
    /// <summary>
    /// Removes a remote SFTP file.
    /// </summary>

    public static TransferettoOperationResult RemoveSftpFile(TransferettoSftpSession session, string remotePath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        session.Client.DeleteFile(remotePath);
        return new TransferettoOperationResult {
            Action = "RemoveFile",
            Status = true,
            Path = remotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Renames a remote SFTP file.
    /// </summary>

    public static TransferettoOperationResult RenameSftpFile(TransferettoSftpSession session, string sourcePath, string destinationPath) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        session.Client.RenameFile(sourcePath, destinationPath);
        return new TransferettoOperationResult {
            Action = "RenameFile",
            Status = true,
            OldPath = sourcePath,
            NewPath = destinationPath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Moves a remote SFTP file.
    /// </summary>

    public static TransferettoOperationResult MoveSftpFile(
        TransferettoSftpSession session,
        string sourcePath,
        string destinationPath,
        bool posixRename = false) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        session.Client.RenameFile(sourcePath, destinationPath, posixRename);
        return new TransferettoOperationResult {
            Action = "MoveFile",
            Status = true,
            OldPath = sourcePath,
            NewPath = destinationPath,
            Message = posixRename ? "POSIX rename" : string.Empty
        };
    }
    /// <summary>
    /// Moves a remote SFTP directory.
    /// </summary>

    public static TransferettoOperationResult MoveSftpDirectory(
        TransferettoSftpSession session,
        string sourcePath,
        string destinationPath,
        bool posixRename = false) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        EnsureNotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        session.Client.RenameFile(sourcePath, destinationPath, posixRename);
        return new TransferettoOperationResult {
            Action = "MoveDirectory",
            Status = true,
            OldPath = sourcePath,
            NewPath = destinationPath,
            Message = posixRename ? "POSIX rename" : string.Empty
        };
    }
    /// <summary>
    /// Read Sftp Text.
    /// </summary>

    public static string ReadSftpText(TransferettoSftpSession session, string path, Encoding? encoding = null) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        return encoding is null
            ? session.Client.ReadAllText(path)
            : session.Client.ReadAllText(path, encoding);
    }
    /// <summary>
    /// Read Sftp Bytes.
    /// </summary>

    public static byte[] ReadSftpBytes(TransferettoSftpSession session, string path) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        return session.Client.ReadAllBytes(path);
    }
    /// <summary>
    /// Write Sftp Text.
    /// </summary>

    public static TransferettoOperationResult WriteSftpText(TransferettoSftpSession session, string path, string content, bool append, Encoding? encoding = null) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        EnsureNotNull(content, nameof(content));

        if (append) {
            if (encoding is null) {
                session.Client.AppendAllText(path, content);
            } else {
                session.Client.AppendAllText(path, content, encoding);
            }
        } else {
            if (encoding is null) {
                session.Client.WriteAllText(path, content);
            } else {
                session.Client.WriteAllText(path, content, encoding);
            }
        }

        return new TransferettoOperationResult {
            Action = append ? "AppendText" : "WriteText",
            Status = true,
            Path = path,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Write Sftp Bytes.
    /// </summary>

    public static TransferettoOperationResult WriteSftpBytes(TransferettoSftpSession session, string path, byte[] content) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));
        EnsureNotNull(content, nameof(content));

        session.Client.WriteAllBytes(path, content);
        return new TransferettoOperationResult {
            Action = "WriteBytes",
            Status = true,
            Path = path,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Opens a streamed SFTP file session.
    /// </summary>

    public static TransferettoSftpStreamSession OpenSftpStream(
        TransferettoSftpSession session,
        string path,
        TransferettoSftpStreamMode mode = TransferettoSftpStreamMode.Read) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        SftpFileStream stream = mode switch {
            TransferettoSftpStreamMode.Write => session.Client.OpenWrite(path),
            TransferettoSftpStreamMode.Append => session.Client.Open(path, FileMode.Append, FileAccess.Write),
            _ => session.Client.OpenRead(path)
        };

        return new TransferettoSftpStreamSession(session, stream, path, mode);
    }
    /// <summary>
    /// Reads a chunk from an SFTP stream.
    /// </summary>

    public static TransferettoSftpStreamReadResult ReadSftpStream(TransferettoSftpStreamSession session, int count = 4096) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The SFTP stream is not readable.");
        }

        int resolvedCount = count > 0 ? count : 4096;
        byte[] buffer = new byte[resolvedCount];
        int bytesRead = session.Stream.Read(buffer, 0, buffer.Length);
        if (bytesRead != buffer.Length) {
            Array.Resize(ref buffer, bytesRead);
        }

        return new TransferettoSftpStreamReadResult {
            Status = true,
            Path = session.RemotePath,
            Data = buffer,
            BytesRead = bytesRead,
            Position = session.Position,
            EndOfStream = bytesRead == 0 || session.Position >= session.Length
        };
    }
    /// <summary>
    /// Writes a chunk to an SFTP stream.
    /// </summary>

    public static TransferettoSftpStreamWriteResult WriteSftpStream(
        TransferettoSftpStreamSession session,
        byte[] content,
        bool flush = false) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The SFTP stream is not writable.");
        }

        session.Stream.Write(content, 0, content.Length);
        if (flush) {
            session.Stream.Flush();
        }

        return new TransferettoSftpStreamWriteResult {
            Action = "WriteStream",
            Status = true,
            Path = session.RemotePath,
            BytesWritten = content.Length,
            Position = session.Position,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Moves the current position within an SFTP stream.
    /// </summary>

    public static long SeekSftpStream(TransferettoSftpStreamSession session, long offset, SeekOrigin origin = SeekOrigin.Begin) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);

        if (!session.CanSeek) {
            throw new InvalidOperationException("The SFTP stream does not support seeking.");
        }

        return session.Stream.Seek(offset, origin);
    }
    /// <summary>
    /// Flushes buffered data for an SFTP stream.
    /// </summary>

    public static TransferettoOperationResult FlushSftpStream(TransferettoSftpStreamSession session) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);

        session.Stream.Flush();
        return new TransferettoOperationResult {
            Action = "FlushStream",
            Status = true,
            Path = session.RemotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Closes an SFTP stream session.
    /// </summary>

    public static void CloseSftpStream(TransferettoSftpStreamSession session) {
        EnsureNotNull(session, nameof(session));
        session.Dispose();
    }

    private static SftpClient CreateSftpClient(TransferettoSftpConnectionOptions options) {
        return new SftpClient(CreateSshConnectionInfo(options));
    }

    private static long ReportTransferProgress(
        TransferettoTransferOptions options,
        string action,
        string protocol,
        TransferettoTransferDirection direction,
        string? localPath,
        string? remotePath,
        ulong transferredBytes,
        long? totalBytes,
        long lastReportedBytes,
        bool force = false) {
        options.CancellationToken.ThrowIfCancellationRequested();

        long currentBytes = transferredBytes > long.MaxValue ? long.MaxValue : (long) transferredBytes;
        long interval = options.ProgressIntervalBytes > 0 ? options.ProgressIntervalBytes : 65536;
        if (!force && (!totalBytes.HasValue || currentBytes < totalBytes.Value) && currentBytes - lastReportedBytes < interval) {
            return lastReportedBytes;
        }

        options.Progress?.Report(new TransferettoTransferProgress {
            Action = action,
            Protocol = protocol,
            Direction = direction,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = currentBytes,
            TotalBytes = totalBytes
        });
        return currentBytes;
    }

    private static void ApplySftpClientOptions(SftpClient client, TransferettoSftpConnectionOptions options) {
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

    private static void EnsureFileExists(string path, string paramName) {
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"File {path} does not exist.", path);
        }
    }

    private static void WriteLocalFileAtomically(string localPath, Action<FileStream> writer) {
        string temporaryPath = CreateTemporaryLocalTransferPath(localPath);

        try {
            using (FileStream fileStream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                writer(fileStream);
                fileStream.Flush();
            }

            if (File.Exists(localPath)) {
                File.Delete(localPath);
            }

            File.Move(temporaryPath, localPath);
        } catch {
            TryDeleteLocalFile(temporaryPath);
            throw;
        }
    }

    private static string CreateTemporaryLocalTransferPath(string localPath) {
        string? directory = Path.GetDirectoryName(localPath);
        string fileName = Path.GetFileName(localPath);
        string temporaryFileName = $"{fileName}.transferetto.{Guid.NewGuid():N}.part";

        return string.IsNullOrWhiteSpace(directory)
            ? temporaryFileName
            : Path.Combine(directory, temporaryFileName);
    }

    private static void TryDeleteLocalFile(string path) {
        try {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        } catch {
            // Best-effort cleanup: preserve the original transfer error.
        }
    }

    private static void EnsureOpenSftpStream(TransferettoSftpStreamSession session) {
        if (session.IsDisposed) {
            throw new ObjectDisposedException(nameof(TransferettoSftpStreamSession), "The SFTP stream has already been closed.");
        }
    }

    private static short ParseSftpPermissions(string permissions) {
        string normalized = permissions.Trim();
        if (normalized.StartsWith("0", StringComparison.Ordinal) && normalized.Length > 3) {
            normalized = normalized.TrimStart('0');
        }

        if (normalized.Length != 3 || normalized.Any(static digit => digit < '0' || digit > '7')) {
            throw new ArgumentOutOfRangeException(nameof(permissions), permissions, "Permissions must be a three-digit octal string such as 644 or 755.");
        }

        return Convert.ToInt16(normalized, 8);
    }

    private static void ValidatePermissionDigit(int value, string paramName) {
        if (value < 0 || value > 7) {
            throw new ArgumentOutOfRangeException(paramName, value, "Permission digit must be between 0 and 7.");
        }
    }

    private static void UploadSftpDirectoryInternal(
        TransferettoSftpSession session,
        DirectoryInfo localDirectory,
        string remoteDirectoryPath,
        bool allowOverride,
        TransferettoTransferOptions options,
        ICollection<TransferettoTransferResult> results) {
        options.CancellationToken.ThrowIfCancellationRequested();
        foreach (FileInfo file in localDirectory.GetFiles()) {
            options.CancellationToken.ThrowIfCancellationRequested();
            string remoteFilePath = CombineRemotePath(remoteDirectoryPath, file.Name);
            results.Add(UploadSftpFile(session, file.FullName, remoteFilePath, allowOverride, options));
        }

        foreach (DirectoryInfo directory in localDirectory.GetDirectories()) {
            options.CancellationToken.ThrowIfCancellationRequested();
            string remoteChildPath = CombineRemotePath(remoteDirectoryPath, directory.Name);
            EnsureSftpDirectoryExists(session, remoteChildPath, results, options);
            UploadSftpDirectoryInternal(session, directory, remoteChildPath, allowOverride, options, results);
        }
    }

    private static void DownloadSftpDirectoryInternal(
        TransferettoSftpSession session,
        string remoteDirectoryPath,
        string localDirectoryPath,
        bool allowOverride,
        TransferettoTransferOptions options,
        ICollection<TransferettoTransferResult> results) {
        options.CancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TransferettoSftpItem> items = GetSftpListing(session, remoteDirectoryPath);
        foreach (TransferettoSftpItem item in items.Where(static item => !IsSpecialSftpDirectory(item))) {
            options.CancellationToken.ThrowIfCancellationRequested();
            if (item.IsDirectory) {
                string localChildPath = Path.Combine(localDirectoryPath, item.Name);
                Directory.CreateDirectory(localChildPath);
                results.Add(CreateDirectoryTransferResult(localChildPath, item.FullName));
                DownloadSftpDirectoryInternal(session, item.FullName, localChildPath, allowOverride, options, results);
                continue;
            }

            if (!item.IsRegularFile) {
                continue;
            }

            string localFilePath = Path.Combine(localDirectoryPath, item.Name);
            if (File.Exists(localFilePath) && !allowOverride) {
                results.Add(new TransferettoTransferResult {
                    Action = "DownloadFile",
                    Status = true,
                    IsSuccess = true,
                    IsSkipped = true,
                    IsSkippedByRule = false,
                    IsFailed = false,
                    LocalPath = localFilePath,
                    RemotePath = item.FullName,
                    Message = "Skipped"
                });
                continue;
            }

            results.Add(DownloadSftpFile(session, item.FullName, localFilePath, options));
        }
    }

    private static void EnsureSftpDirectoryExists(
        TransferettoSftpSession session,
        string path,
        ICollection<TransferettoTransferResult>? results = null,
        TransferettoTransferOptions? options = null) {
        options?.CancellationToken.ThrowIfCancellationRequested();
        string normalizedPath = NormalizeRemotePath(path);
        if (string.IsNullOrWhiteSpace(normalizedPath) || normalizedPath == "/" || normalizedPath == ".") {
            return;
        }

        if (session.Client.Exists(normalizedPath)) {
            if (!session.Client.GetAttributes(normalizedPath).IsDirectory) {
                throw new InvalidOperationException($"Remote path {normalizedPath} exists but is not a directory.");
            }

            return;
        }

        string parent = GetRemoteParent(normalizedPath);
        if (!string.IsNullOrWhiteSpace(parent) && parent != normalizedPath) {
            EnsureSftpDirectoryExists(session, parent, results, options);
        }

        options?.CancellationToken.ThrowIfCancellationRequested();
        session.Client.CreateDirectory(normalizedPath);
        results?.Add(CreateDirectoryTransferResult(null, normalizedPath));
    }

    private static TransferettoTransferResult CreateDirectoryTransferResult(string? localPath, string remotePath) {
        return new TransferettoTransferResult {
            Action = "CreateDirectory",
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

    private static bool IsSpecialSftpDirectory(TransferettoSftpItem item) {
        return item.Name is "." or "..";
    }

    private static string NormalizeRemotePath(string path) {
        string normalized = path.Replace('\\', '/').Trim();
        if (normalized.Length > 1) {
            normalized = normalized.TrimEnd('/');
        }

        return normalized;
    }

    private static string CombineRemotePath(string basePath, string childPath) {
        string normalizedBasePath = NormalizeRemotePath(basePath);
        string normalizedChildPath = NormalizeRemotePath(childPath).TrimStart('/');
        if (normalizedBasePath == "/") {
            return "/" + normalizedChildPath;
        }

        return string.IsNullOrWhiteSpace(normalizedBasePath)
            ? normalizedChildPath
            : normalizedBasePath + "/" + normalizedChildPath;
    }

    private static string GetRemoteParent(string path) {
        string normalizedPath = NormalizeRemotePath(path);
        int separatorIndex = normalizedPath.LastIndexOf('/');
        if (separatorIndex < 0) {
            return string.Empty;
        }

        return separatorIndex == 0
            ? "/"
            : normalizedPath.Substring(0, separatorIndex);
    }
}
