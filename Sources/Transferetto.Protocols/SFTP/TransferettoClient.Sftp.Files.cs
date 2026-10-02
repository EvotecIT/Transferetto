using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
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
        return UploadSftpFileCoreAsync(session, localPath, remotePath, allowOverride, options).GetAwaiter().GetResult();
    }

    private static async Task<TransferettoTransferResult> UploadSftpFileCoreAsync(
        TransferettoSftpSession session, string localPath, string remotePath, bool allowOverride, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        EnsureFileExists(localPath, nameof(localPath));
        FileInfo fileInfo = new(localPath);
        long totalBytes = fileInfo.Length;
        DateTime startedUtc = DateTime.UtcNow;
        long bytesTransferred = 0;
        using FileStream fileStream = new(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        string temporaryPath = ProtocolTransferEndpointPath.CreateTemporaryPath(remotePath);
        try {
            using TransferettoProgressStream progressContent = new(fileStream, transferredBytes => {
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
            // Own the remote handle so cancellation also closes it before removing staging.
            using (Stream remoteStream = await session.Client.OpenAsync(temporaryPath, FileMode.CreateNew, FileAccess.Write,
                resolvedOptions.CancellationToken).ConfigureAwait(false)) {
                await TransferContent.CopyToAsync(progressContent, remoteStream, totalBytes, resolvedOptions.CancellationToken).ConfigureAwait(false);
                await remoteStream.FlushAsync(resolvedOptions.CancellationToken).ConfigureAwait(false);
            }
            resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
            ProtocolTransferCommit.Commit(new SftpTransferCommitOperations(session), temporaryPath, remotePath, remotePath,
                allowOverride ? Transferetto.Core.TransferWriteMode.Overwrite : Transferetto.Core.TransferWriteMode.FailIfExists, "SFTP");
        } catch {
            try { if (session.Client.Exists(temporaryPath)) { session.Client.DeleteFile(temporaryPath); } }
            catch { /* Preserve the upload or commit failure. */ }
            throw;
        }
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
        return DownloadSftpFileCoreAsync(session, remotePath, localPath, options).GetAwaiter().GetResult();
    }

    private static async Task<TransferettoTransferResult> DownloadSftpFileCoreAsync(
        TransferettoSftpSession session, string remotePath, string localPath, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));

        TransferettoTransferOptions resolvedOptions = options ?? new TransferettoTransferOptions();
        resolvedOptions.CancellationToken.ThrowIfCancellationRequested();
        long totalBytes = (await session.Client.GetAttributesAsync(remotePath, resolvedOptions.CancellationToken).ConfigureAwait(false)).Size;
        DateTime startedUtc = DateTime.UtcNow;
        long bytesTransferred = 0;
        EnsureSafeLocalFilePath(localPath);
        string? directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        await WriteLocalFileAtomicallyAsync(localPath, async fileStream => {
            using TransferettoProgressStream progressContent = new(fileStream, transferredBytes => {
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
            await session.Client.DownloadFileAsync(remotePath, progressContent, downloadProgress: null,
                resolvedOptions.CancellationToken).ConfigureAwait(false);
        }, resolvedOptions.CancellationToken).ConfigureAwait(false);
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

    private static async Task WriteLocalFileAtomicallyAsync(string localPath, Func<FileStream, Task> writer, CancellationToken cancellationToken) {
        EnsureSafeLocalFilePath(localPath);
        string temporaryPath = CreateTemporaryLocalTransferPath(localPath);
        try {
            using (FileStream fileStream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)) {
                await writer(fileStream).ConfigureAwait(false);
                await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            TransferFileSystem.CommitStagedFile(temporaryPath, localPath);
        } finally { TryDeleteLocalFile(temporaryPath); }
    }
}
