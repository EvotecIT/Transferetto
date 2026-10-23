using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentFTP;
using FluentFTP.Exceptions;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>Downloads an FTP/FTPS file, preserving its original content if receipt fails.</summary>
    public static TransferettoTransferResult DownloadFtpFile(TransferettoFtpSession session, string localPath,
        string remotePath, FtpLocalExists localExists = FtpLocalExists.Skip, FtpVerify verifyOptions = FtpVerify.None) =>
        DownloadFtpFile(session, localPath, remotePath, localExists, verifyOptions, null);

    /// <summary>Stages an FTP/FTPS file before publishing it, with cancellation and logical-path progress.</summary>
    public static TransferettoTransferResult DownloadFtpFile(TransferettoFtpSession session, string localPath,
        string remotePath, FtpLocalExists localExists, FtpVerify verifyOptions, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        options ??= new TransferettoTransferOptions();
        options.CancellationToken.ThrowIfCancellationRequested();
        EnsureSafeLocalFilePath(localPath);
        string fullPath = Path.GetFullPath(localPath);
        using LocalDirectoryTransaction transaction = new(Path.GetDirectoryName(fullPath)!, "ftp");
        TransferettoTransferResult result = ReceiveFtpFile(session, transaction, Path.GetFileName(fullPath),
            localPath, remotePath, localExists, verifyOptions, options);
        if (result.IsSuccess && !result.IsSkipped) {
            transaction.CommitAsync(options.CancellationToken).GetAwaiter().GetResult();
        }
        return result;
    }

    /// <summary>Downloads files into one directory, observing the selected batch error policy.</summary>
    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpFiles(TransferettoFtpSession session,
        string localPath, IEnumerable<string> remotePaths, FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None, FtpError errorHandling = FtpError.Stop) =>
        DownloadFtpFiles(session, localPath, remotePaths, localExists, verifyOptions, errorHandling, null);

    /// <summary>Downloads files with protected receipt and native transfer/verification options.</summary>
    /// <remarks>None continues, Stop returns after failure, and Throw raises an FTP exception. DeleteProcessed
    /// withholds publication until the batch succeeds, so failure or cancellation preserves original files.</remarks>
    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpFiles(TransferettoFtpSession session,
        string localPath, IEnumerable<string> remotePaths, FtpLocalExists localExists, FtpVerify verifyOptions,
        FtpError errorHandling, TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNull(remotePaths, nameof(remotePaths));
        if (errorHandling.HasFlag(FtpError.Stop) && errorHandling.HasFlag(FtpError.Throw)) {
            throw new ArgumentException("FtpError.Throw and FtpError.Stop cannot be combined.", nameof(errorHandling));
        }
        options ??= new TransferettoTransferOptions();
        options.CancellationToken.ThrowIfCancellationRequested();
        string[] paths = remotePaths.Where(static path => !string.IsNullOrWhiteSpace(path)).ToArray();
        if (paths.Length == 0) { return Array.Empty<TransferettoTransferResult>(); }
        TransferFileSystem.EnsureNoLinkTraversal(localPath, localPath);
        TransferPathRegistry names = new(localPath);
        string[] relatives = paths.Select(path => path.Substring(path.LastIndexOf('/') + 1)).ToArray();
        string[] destinations = relatives.Select(names.Add).ToArray();
        bool rollbackBatch = errorHandling.HasFlag(FtpError.DeleteProcessed);
        using LocalDirectoryTransaction transaction = new(localPath, "ftp");
        List<TransferettoTransferResult> results = new();
        for (int index = 0; index < paths.Length; index++) {
            options.CancellationToken.ThrowIfCancellationRequested();
            TransferettoTransferResult result;
            Exception? failure = null;
            try {
                result = rollbackBatch
                    ? ReceiveFtpFile(session, transaction, relatives[index], destinations[index], paths[index], localExists, verifyOptions, options)
                    : DownloadFtpFile(session, destinations[index], paths[index], localExists, verifyOptions, options);
            } catch (OperationCanceledException) { throw; }
            catch (Exception exception) {
                options.CancellationToken.ThrowIfCancellationRequested();
                failure = exception;
                result = FailedFtpDownload(FromDownloadStatus(destinations[index], paths[index], FtpStatus.Failed), exception.Message);
            }
            results.Add(result);
            if (!result.IsFailed) { continue; }
            if (errorHandling.HasFlag(FtpError.Throw)) {
                throw new FtpException("An error occurred downloading file(s): " + paths[index] + ". " + result.Message, failure);
            }
            if (errorHandling.HasFlag(FtpError.Stop)) {
                for (int pending = index + 1; pending < paths.Length; pending++) {
                    results.Add(FailedFtpDownload(FromDownloadStatus(destinations[pending], paths[pending], FtpStatus.Failed),
                        "Not attempted because an earlier FTP download failed."));
                }
                break;
            }
        }
        options.CancellationToken.ThrowIfCancellationRequested();
        if (rollbackBatch) {
            if (results.Any(result => result.IsFailed)) {
                return results.Select(result => result.IsSuccess && !result.IsSkipped
                    ? FailedFtpDownload(result, "The batch download was not published because another member failed.") : result).ToArray();
            }
            if (results.Any(result => result.IsSuccess && !result.IsSkipped)) {
                transaction.CommitAsync(options.CancellationToken).GetAwaiter().GetResult();
            }
        }
        return results;
    }

    /// <summary>Uses the configured FTP engine to receive one file into the transaction's private staging.</summary>
    private static TransferettoTransferResult ReceiveFtpFile(TransferettoFtpSession session,
        LocalDirectoryTransaction transaction, string relative, string localPath, string remotePath,
        FtpLocalExists localExists, FtpVerify verifyOptions, TransferettoTransferOptions options) {
        options.CancellationToken.ThrowIfCancellationRequested();
        long remoteSize = GetFtpFileSize(session, remotePath, -1);
        long? total = remoteSize >= 0 ? remoteSize : null;
        DateTime started = DateTime.UtcNow;
        if ((localExists == FtpLocalExists.Skip && File.Exists(localPath)) ||
            (!session.Client.Config.DownloadZeroByteFiles && remoteSize == 0)) {
            return FromDownloadStatus(localPath, remotePath, FtpStatus.Skipped, 0, total, started, DateTime.UtcNow);
        }
        long bytes = 0;
        long lastReported = 0;
        FtpStatus status = FtpStatus.Failed;
        string? lengthFailure = null;
        try {
            transaction.AddFile(relative, staged => {
                status = session.Client.DownloadFile(staged, remotePath, localExists, verifyOptions, progress => {
                    options.CancellationToken.ThrowIfCancellationRequested();
                    bytes = Math.Max(bytes, NormalizeTransferredBytes(progress.TransferredBytes));
                    lastReported = ReportFtpTransferProgress(options, "DownloadFile", TransferettoTransferDirection.Download,
                        localPath, remotePath, bytes, ResolveFtpTotalBytes(progress, total), lastReported);
                });
                options.CancellationToken.ThrowIfCancellationRequested();
                try { status = CompleteFtpReceive(staged, remotePath, remoteSize, session.Client.Config.DownloadDataType,
                    session.Client.Config.DownloadZeroByteFiles, status); }
                catch (EndOfStreamException exception) { status = FtpStatus.Failed; lengthFailure = exception.Message; return false; }
                if (status != FtpStatus.Success) { return false; }
                bytes = Math.Max(bytes, new FileInfo(staged).Length);
                return true;
            }, seedExisting: localExists != FtpLocalExists.Overwrite && localExists != FtpLocalExists.Skip,
                cancellationToken: options.CancellationToken);
        } catch {
            options.CancellationToken.ThrowIfCancellationRequested();
            throw;
        }
        if (status == FtpStatus.Success) {
            ReportFtpTransferProgress(options, "DownloadFile", TransferettoTransferDirection.Download,
                localPath, remotePath, bytes, total, lastReported, force: true);
            options.CancellationToken.ThrowIfCancellationRequested();
        }
        TransferettoTransferResult result = FromDownloadStatus(localPath, remotePath, status, bytes, total, started, DateTime.UtcNow);
        return lengthFailure != null ? FailedFtpDownload(result, lengthFailure) : result;
    }

    /// <summary>Checks native completion before publication and recognizes an intentionally omitted empty file.</summary>
    private static FtpStatus CompleteFtpReceive(string staged, string remote, long expectedSize,
        FtpDataType dataType, bool downloadZeroByteFiles, FtpStatus status) {
        if (status != FtpStatus.Success) { return status; }
        if (!downloadZeroByteFiles && expectedSize <= 0 && !File.Exists(staged)) { return FtpStatus.Skipped; }
        // ASCII transformations and servers without a size cannot support a byte-length comparison.
        if (dataType == FtpDataType.Binary && expectedSize >= 0) {
            long actualSize = new FileInfo(staged).Length;
            if (actualSize != expectedSize) {
                throw new EndOfStreamException("The FTP download length does not match the remote file: " + remote +
                    " (expected " + expectedSize + ", received " + actualSize + " bytes).");
            }
        }
        return status;
    }

    private static TransferettoTransferResult FailedFtpDownload(TransferettoTransferResult result, string message) => new() {
        Action = "DownloadFile", Status = false, IsSuccess = false, IsFailed = true,
        LocalPath = result.LocalPath, RemotePath = result.RemotePath, Message = message,
        BytesTransferred = result.BytesTransferred, TotalBytes = result.TotalBytes,
        StartedUtc = result.StartedUtc, CompletedUtc = result.CompletedUtc
    };
}
