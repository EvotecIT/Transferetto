using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto;

/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>
public static partial class TransferettoClient {
    /// <summary>
    /// Asynchronously uploads one or more files to an FTP or FTPS server.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> UploadFtpFilesAsync(
        TransferettoFtpSession session,
        string? remotePath,
        IEnumerable<string>? localPaths,
        IEnumerable<FileInfo>? localFiles,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        FtpError errorHandling = FtpError.None,
        bool createRemoteDirectory = false,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadFtpFiles(session, remotePath, localPaths, localFiles, remoteExists, verifyOptions, errorHandling, createRemoteDirectory, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a file from an FTP or FTPS server.
    /// </summary>
    public static Task<TransferettoTransferResult> DownloadFtpFileAsync(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadFtpFile(session, localPath, remotePath, localExists, verifyOptions, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads multiple files from an FTP or FTPS server.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> DownloadFtpFilesAsync(
        TransferettoFtpSession session,
        string localPath,
        IEnumerable<string> remotePaths,
        FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        FtpError errorHandling = FtpError.Stop,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadFtpFiles(session, localPath, remotePaths, localExists, verifyOptions, errorHandling, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously uploads a local directory to an FTP or FTPS server.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> UploadFtpDirectoryAsync(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        IEnumerable<FtpRule>? rules = null,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadFtpDirectory(session, localPath, remotePath, folderSyncMode, remoteExists, verifyOptions, rules, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a remote directory from an FTP or FTPS server.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> DownloadFtpDirectoryAsync(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update,
        FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        IEnumerable<FtpRule>? rules = null,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadFtpDirectory(session, localPath, remotePath, folderSyncMode, localExists, verifyOptions, rules, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously transfers a file directly between two FTP or FTPS sessions.
    /// </summary>
    public static Task<TransferettoTransferResult> StartFxpFileTransferAsync(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        bool createRemoteDirectory = false,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => StartFxpFileTransfer(sourceSession, sourcePath, destinationSession, destinationPath, createRemoteDirectory, remoteExists, verifyOptions, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously transfers a directory directly between two FTP or FTPS sessions.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> StartFxpDirectoryTransferAsync(
        TransferettoFtpSession sourceSession,
        string sourcePath,
        TransferettoFtpSession destinationSession,
        string destinationPath,
        FtpFolderSyncMode folderSyncMode = FtpFolderSyncMode.Update,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        IEnumerable<FtpRule>? rules = null,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => StartFxpDirectoryTransfer(sourceSession, sourcePath, destinationSession, destinationPath, folderSyncMode, remoteExists, verifyOptions, rules, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously uploads a file over SFTP.
    /// </summary>
    public static Task<TransferettoTransferResult> UploadSftpFileAsync(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        bool allowOverride,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadSftpFile(session, localPath, remotePath, allowOverride, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a file over SFTP.
    /// </summary>
    public static Task<TransferettoTransferResult> DownloadSftpFileAsync(
        TransferettoSftpSession session,
        string remotePath,
        string localPath,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadSftpFile(session, remotePath, localPath, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously uploads a directory over SFTP.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> UploadSftpDirectoryAsync(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        bool allowOverride = false,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadSftpDirectory(session, localPath, remotePath, allowOverride, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a directory over SFTP.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoTransferResult>> DownloadSftpDirectoryAsync(
        TransferettoSftpSession session,
        string remotePath,
        string localPath,
        bool allowOverride = false,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadSftpDirectory(session, remotePath, localPath, allowOverride, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously uploads a file over SCP.
    /// </summary>
    public static Task<TransferettoTransferResult> UploadScpFileAsync(
        TransferettoScpSession session,
        string localPath,
        string remotePath,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadScpFile(session, localPath, remotePath, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a file over SCP.
    /// </summary>
    public static Task<TransferettoTransferResult> DownloadScpFileAsync(
        TransferettoScpSession session,
        string remotePath,
        string localPath,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadScpFile(session, remotePath, localPath, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously uploads a directory over SCP.
    /// </summary>
    public static Task<TransferettoTransferResult> UploadScpDirectoryAsync(
        TransferettoScpSession session,
        string localPath,
        string remotePath,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => UploadScpDirectory(session, localPath, remotePath, resolvedOptions),
            options,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously downloads a directory over SCP.
    /// </summary>
    public static Task<TransferettoTransferResult> DownloadScpDirectoryAsync(
        TransferettoScpSession session,
        string remotePath,
        string localPath,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedOptions => DownloadScpDirectory(session, remotePath, localPath, resolvedOptions),
            options,
            cancellationToken);
    }

    private static async Task<T> RunTransferAsync<T>(
        Func<TransferettoTransferOptions?, T> operation,
        TransferettoTransferOptions? options,
        CancellationToken cancellationToken) {
        CancellationTokenSource? linkedCancellationSource = null;

        try {
            TransferettoTransferOptions? resolvedOptions = ResolveAsyncTransferOptions(options, cancellationToken, out linkedCancellationSource);
            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            return await Task.Run(() => operation(resolvedOptions), effectiveCancellationToken).ConfigureAwait(false);
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    private static TransferettoTransferOptions? ResolveAsyncTransferOptions(
        TransferettoTransferOptions? options,
        CancellationToken cancellationToken,
        out CancellationTokenSource? linkedCancellationSource) {
        linkedCancellationSource = null;

        if (!cancellationToken.CanBeCanceled) {
            return options;
        }

        if (options is null) {
            return new TransferettoTransferOptions {
                CancellationToken = cancellationToken
            };
        }

        if (!options.CancellationToken.CanBeCanceled) {
            return CloneTransferOptions(options, cancellationToken);
        }

        linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken, cancellationToken);
        return CloneTransferOptions(options, linkedCancellationSource.Token);
    }

    private static TransferettoTransferOptions CloneTransferOptions(TransferettoTransferOptions options, CancellationToken cancellationToken) {
        return new TransferettoTransferOptions {
            CancellationToken = cancellationToken,
            Progress = options.Progress,
            ProgressIntervalBytes = options.ProgressIntervalBytes
        };
    }

    /// <summary>
    /// Reads a chunk from an FTP or FTPS stream with shared cancellation and progress support.
    /// </summary>
    public static TransferettoFtpStreamReadResult ReadFtpStream(
        TransferettoFtpStreamSession session,
        int count,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The FTP stream is not readable.");
        }

        options?.CancellationToken.ThrowIfCancellationRequested();
        int resolvedCount = count > 0 ? count : 4096;
        byte[] buffer = new byte[resolvedCount];
        int bytesRead = session.Stream.Read(buffer, 0, buffer.Length);
        if (bytesRead != buffer.Length) {
            Array.Resize(ref buffer, bytesRead);
        }

        session.ProcessedBytes += bytesRead;
        options?.CancellationToken.ThrowIfCancellationRequested();
        ReportFtpStreamProgress(session, options, "ReadStream", TransferettoTransferDirection.Download, force: bytesRead == 0);

        long position = session.CanSeek ? session.Position : session.ProcessedBytes;
        return new TransferettoFtpStreamReadResult {
            Status = true,
            Path = session.RemotePath,
            Data = buffer,
            BytesRead = bytesRead,
            Position = position,
            EndOfStream = bytesRead == 0 || (session.CanSeek && session.Position >= session.Length)
        };
    }

    /// <summary>
    /// Writes a chunk to an FTP or FTPS stream with shared cancellation and progress support.
    /// </summary>
    public static TransferettoFtpStreamWriteResult WriteFtpStream(
        TransferettoFtpStreamSession session,
        byte[] content,
        bool flush,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The FTP stream is not writable.");
        }

        options?.CancellationToken.ThrowIfCancellationRequested();
        session.Stream.Write(content, 0, content.Length);
        if (flush) {
            session.Stream.Flush();
        }

        session.ProcessedBytes += content.Length;
        options?.CancellationToken.ThrowIfCancellationRequested();
        ReportFtpStreamProgress(session, options, "WriteStream", TransferettoTransferDirection.Upload, force: flush);

        return new TransferettoFtpStreamWriteResult {
            Status = true,
            Path = session.RemotePath,
            BytesWritten = content.Length,
            Position = session.CanSeek ? session.Position : session.ProcessedBytes,
            Message = string.Empty
        };
    }

    /// <summary>
    /// Asynchronously reads a chunk from an FTP or FTPS stream.
    /// </summary>
    public static async Task<TransferettoFtpStreamReadResult> ReadFtpStreamAsync(
        TransferettoFtpStreamSession session,
        int count = 4096,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The FTP stream is not readable.");
        }

        CancellationTokenSource? linkedCancellationSource = null;
        try {
            TransferettoTransferOptions? resolvedOptions = ResolveAsyncTransferOptions(options, cancellationToken, out linkedCancellationSource);
            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            int resolvedCount = count > 0 ? count : 4096;
            byte[] buffer = new byte[resolvedCount];
            int bytesRead = await session.Stream.ReadAsync(buffer, 0, buffer.Length, effectiveCancellationToken).ConfigureAwait(false);
            if (bytesRead != buffer.Length) {
                Array.Resize(ref buffer, bytesRead);
            }

            session.ProcessedBytes += bytesRead;
            effectiveCancellationToken.ThrowIfCancellationRequested();
            ReportFtpStreamProgress(session, resolvedOptions, "ReadStream", TransferettoTransferDirection.Download, force: bytesRead == 0);

            long position = session.CanSeek ? session.Position : session.ProcessedBytes;
            return new TransferettoFtpStreamReadResult {
                Status = true,
                Path = session.RemotePath,
                Data = buffer,
                BytesRead = bytesRead,
                Position = position,
                EndOfStream = bytesRead == 0 || (session.CanSeek && session.Position >= session.Length)
            };
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously writes a chunk to an FTP or FTPS stream.
    /// </summary>
    public static async Task<TransferettoFtpStreamWriteResult> WriteFtpStreamAsync(
        TransferettoFtpStreamSession session,
        byte[] content,
        bool flush = false,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The FTP stream is not writable.");
        }

        CancellationTokenSource? linkedCancellationSource = null;
        try {
            TransferettoTransferOptions? resolvedOptions = ResolveAsyncTransferOptions(options, cancellationToken, out linkedCancellationSource);
            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            await session.Stream.WriteAsync(content, 0, content.Length, effectiveCancellationToken).ConfigureAwait(false);
            if (flush) {
                await session.Stream.FlushAsync(effectiveCancellationToken).ConfigureAwait(false);
            }

            session.ProcessedBytes += content.Length;
            effectiveCancellationToken.ThrowIfCancellationRequested();
            ReportFtpStreamProgress(session, resolvedOptions, "WriteStream", TransferettoTransferDirection.Upload, force: flush);

            return new TransferettoFtpStreamWriteResult {
                Status = true,
                Path = session.RemotePath,
                BytesWritten = content.Length,
                Position = session.CanSeek ? session.Position : session.ProcessedBytes,
                Message = string.Empty
            };
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    /// <summary>
    /// Reads a chunk from an SFTP stream with shared cancellation and progress support.
    /// </summary>
    public static TransferettoSftpStreamReadResult ReadSftpStream(
        TransferettoSftpStreamSession session,
        int count,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The SFTP stream is not readable.");
        }

        options?.CancellationToken.ThrowIfCancellationRequested();
        int resolvedCount = count > 0 ? count : 4096;
        byte[] buffer = new byte[resolvedCount];
        int bytesRead = session.Stream.Read(buffer, 0, buffer.Length);
        if (bytesRead != buffer.Length) {
            Array.Resize(ref buffer, bytesRead);
        }

        session.ProcessedBytes += bytesRead;
        options?.CancellationToken.ThrowIfCancellationRequested();
        ReportSftpStreamProgress(session, options, "ReadStream", TransferettoTransferDirection.Download, force: bytesRead == 0);

        return new TransferettoSftpStreamReadResult {
            Status = true,
            Path = session.RemotePath,
            Data = buffer,
            BytesRead = bytesRead,
            Position = session.CanSeek ? session.Position : session.ProcessedBytes,
            EndOfStream = bytesRead == 0 || (session.CanSeek && session.Position >= session.Length)
        };
    }

    /// <summary>
    /// Writes a chunk to an SFTP stream with shared cancellation and progress support.
    /// </summary>
    public static TransferettoSftpStreamWriteResult WriteSftpStream(
        TransferettoSftpStreamSession session,
        byte[] content,
        bool flush,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The SFTP stream is not writable.");
        }

        options?.CancellationToken.ThrowIfCancellationRequested();
        session.Stream.Write(content, 0, content.Length);
        if (flush) {
            session.Stream.Flush();
        }

        session.ProcessedBytes += content.Length;
        options?.CancellationToken.ThrowIfCancellationRequested();
        ReportSftpStreamProgress(session, options, "WriteStream", TransferettoTransferDirection.Upload, force: flush);

        return new TransferettoSftpStreamWriteResult {
            Action = "WriteStream",
            Status = true,
            Path = session.RemotePath,
            BytesWritten = content.Length,
            Position = session.CanSeek ? session.Position : session.ProcessedBytes,
            Message = string.Empty
        };
    }

    /// <summary>
    /// Asynchronously reads a chunk from an SFTP stream.
    /// </summary>
    public static async Task<TransferettoSftpStreamReadResult> ReadSftpStreamAsync(
        TransferettoSftpStreamSession session,
        int count = 4096,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The SFTP stream is not readable.");
        }

        CancellationTokenSource? linkedCancellationSource = null;
        try {
            TransferettoTransferOptions? resolvedOptions = ResolveAsyncTransferOptions(options, cancellationToken, out linkedCancellationSource);
            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            int resolvedCount = count > 0 ? count : 4096;
            byte[] buffer = new byte[resolvedCount];
            int bytesRead = await session.Stream.ReadAsync(buffer, 0, buffer.Length, effectiveCancellationToken).ConfigureAwait(false);
            if (bytesRead != buffer.Length) {
                Array.Resize(ref buffer, bytesRead);
            }

            session.ProcessedBytes += bytesRead;
            effectiveCancellationToken.ThrowIfCancellationRequested();
            ReportSftpStreamProgress(session, resolvedOptions, "ReadStream", TransferettoTransferDirection.Download, force: bytesRead == 0);

            return new TransferettoSftpStreamReadResult {
                Status = true,
                Path = session.RemotePath,
                Data = buffer,
                BytesRead = bytesRead,
                Position = session.CanSeek ? session.Position : session.ProcessedBytes,
                EndOfStream = bytesRead == 0 || (session.CanSeek && session.Position >= session.Length)
            };
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously writes a chunk to an SFTP stream.
    /// </summary>
    public static async Task<TransferettoSftpStreamWriteResult> WriteSftpStreamAsync(
        TransferettoSftpStreamSession session,
        byte[] content,
        bool flush = false,
        TransferettoTransferOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenSftpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The SFTP stream is not writable.");
        }

        CancellationTokenSource? linkedCancellationSource = null;
        try {
            TransferettoTransferOptions? resolvedOptions = ResolveAsyncTransferOptions(options, cancellationToken, out linkedCancellationSource);
            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            await session.Stream.WriteAsync(content, 0, content.Length, effectiveCancellationToken).ConfigureAwait(false);
            if (flush) {
                await session.Stream.FlushAsync(effectiveCancellationToken).ConfigureAwait(false);
            }

            session.ProcessedBytes += content.Length;
            effectiveCancellationToken.ThrowIfCancellationRequested();
            ReportSftpStreamProgress(session, resolvedOptions, "WriteStream", TransferettoTransferDirection.Upload, force: flush);

            return new TransferettoSftpStreamWriteResult {
                Action = "WriteStream",
                Status = true,
                Path = session.RemotePath,
                BytesWritten = content.Length,
                Position = session.CanSeek ? session.Position : session.ProcessedBytes,
                Message = string.Empty
            };
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    private static void ReportFtpStreamProgress(
        TransferettoFtpStreamSession session,
        TransferettoTransferOptions? options,
        string action,
        TransferettoTransferDirection direction,
        bool force = false) {
        if (options is null) {
            return;
        }

        long transferredBytes = ResolveStreamTransferredBytes(session.OpenedPosition, session.CanSeek ? session.Position : session.ProcessedBytes);
        long? totalBytes = direction == TransferettoTransferDirection.Download
            ? ResolveStreamTotalBytes(session.OpenedPosition, session.CanSeek ? session.Length : (long?) null)
            : null;

        session.LastReportedBytes = ReportTransferProgress(
            options,
            action,
            "FTP",
            direction,
            null,
            session.RemotePath,
            (ulong) transferredBytes,
            totalBytes,
            session.LastReportedBytes,
            force);
    }

    private static void ReportSftpStreamProgress(
        TransferettoSftpStreamSession session,
        TransferettoTransferOptions? options,
        string action,
        TransferettoTransferDirection direction,
        bool force = false) {
        if (options is null) {
            return;
        }

        long transferredBytes = ResolveStreamTransferredBytes(session.OpenedPosition, session.CanSeek ? session.Position : session.ProcessedBytes);
        long? totalBytes = direction == TransferettoTransferDirection.Download
            ? ResolveStreamTotalBytes(session.OpenedPosition, session.CanSeek ? session.Length : (long?) null)
            : null;

        session.LastReportedBytes = ReportTransferProgress(
            options,
            action,
            "SFTP",
            direction,
            null,
            session.RemotePath,
            (ulong) transferredBytes,
            totalBytes,
            session.LastReportedBytes,
            force);
    }

    private static long ResolveStreamTransferredBytes(long openedPosition, long currentPosition) {
        return currentPosition <= openedPosition
            ? 0
            : NormalizeTransferredBytes(currentPosition - openedPosition);
    }

    private static long? ResolveStreamTotalBytes(long openedPosition, long? length) {
        if (!length.HasValue) {
            return null;
        }

        if (length.Value <= openedPosition) {
            return 0;
        }

        return length.Value - openedPosition;
    }
}
