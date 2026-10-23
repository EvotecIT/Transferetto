using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentFTP;
using FluentFTP.Exceptions;
using FluentFTP.Rules;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>Receives the selected files before publishing replacements or removing mirror extras.</summary>
    internal static IReadOnlyList<TransferettoTransferResult> DownloadFtpDirectoryCore(
        IFtpDirectoryDownloadOperations operations, string localRoot, string remoteRoot,
        FtpFolderSyncMode mode, FtpLocalExists existsMode, FtpVerify verifyOptions,
        IEnumerable<FtpRule>? rules, TransferettoTransferOptions options) {
        options.CancellationToken.ThrowIfCancellationRequested();
        if (!operations.DirectoryExists(remoteRoot)) {
            throw new FtpMissingObjectException("Cannot download a missing directory: " + remoteRoot, null, remoteRoot, FtpObjectType.Directory);
        }
        EnsureSafeLocalDirectoryTree(localRoot, options.CancellationToken);
        var manifest = DirectoryDownloadManifest.Build(remoteRoot, localRoot, operations.ListDirectory,
            item => item.Name, item => item.Type == FtpObjectType.Directory,
            item => item.Type == FtpObjectType.File, options.CancellationToken);
        List<FtpRule>? filters = rules?.ToList();
        List<FtpResult> results = new();
        HashSet<string> retained = new(StringComparer.Ordinal);
        // Capture extras before creating our private staging directory inside an existing root.
        string[] localFiles = mode == FtpFolderSyncMode.Mirror && Directory.Exists(localRoot)
            ? TransferFileSystem.EnumerateEntries(localRoot, cancellationToken: options.CancellationToken)
                .OfType<FileInfo>().Select(item => item.FullName).ToArray() : Array.Empty<string>();
        using LocalDirectoryTransaction transaction = new(localRoot, "ftp");
        transaction.AddDirectory("", null, null);
        foreach (var member in manifest) {
            options.CancellationToken.ThrowIfCancellationRequested();
            FtpResult result = new() { Name = member.Metadata.Name, Size = member.Metadata.Size,
                Type = member.Metadata.Type, LocalPath = member.Local, RemotePath = member.Remote, IsDownload = true };
            results.Add(result);
            if (filters != null && !FtpRule.IsAllAllowed(filters, member.Metadata)) {
                result.IsSkipped = true; result.IsSkippedByRule = true; continue;
            }
            retained.Add(TransferFileSystem.GetPathIdentity(member.Local));
            if (member.IsDirectory) {
                transaction.AddDirectory(member.Relative, null, null);
                result.IsSuccess = true; result.IsSkipped = Directory.Exists(member.Local);
                continue;
            }
            if (existsMode == FtpLocalExists.Skip && File.Exists(member.Local)) {
                result.IsSkipped = true; continue;
            }
            long lastReported = 0;
            try {
                FtpStatus status = FtpStatus.Failed;
                transaction.AddFile(member.Relative, staged => {
                    status = operations.DownloadFile(staged, member.Remote, existsMode, verifyOptions, progress => {
                        options.CancellationToken.ThrowIfCancellationRequested();
                        lastReported = ReportFtpTransferProgress(options, "DownloadDirectory", TransferettoTransferDirection.Download,
                            member.Local, member.Remote, progress, member.Metadata.Size >= 0 ? member.Metadata.Size : null, lastReported);
                    });
                    options.CancellationToken.ThrowIfCancellationRequested();
                    return status == FtpStatus.Success;
                }, seedExisting: existsMode != FtpLocalExists.Overwrite && existsMode != FtpLocalExists.Skip,
                    cancellationToken: options.CancellationToken);
                result.IsSuccess = status == FtpStatus.Success;
                result.IsSkipped = status == FtpStatus.Skipped;
                result.IsFailed = status != FtpStatus.Success && status != FtpStatus.Skipped;
                if (result.IsFailed) { result.Exception = new IOException("The FTP file download did not complete: " + member.Remote); }
            } catch (OperationCanceledException) { throw; }
            catch (Exception exception) { result.IsFailed = true; result.Exception = exception; }
        }
        options.CancellationToken.ThrowIfCancellationRequested();
        if (results.Any(result => result.IsFailed)) {
            foreach (FtpResult result in results.Where(result => result.IsSuccess && !result.IsSkipped)) {
                result.IsSuccess = false; result.IsFailed = true;
                result.Exception = new IOException("The directory download was not published because another member failed.");
            }
            return results.Select(result => FromFtpResult("DownloadDirectory", localRoot, result)).ToArray();
        }
        foreach (string file in localFiles) {
            options.CancellationToken.ThrowIfCancellationRequested();
            if (retained.Contains(TransferFileSystem.GetPathIdentity(file))) { continue; }
            FtpListItem localMetadata = new FtpResult { Type = FtpObjectType.File, Size = 0,
                Name = Path.GetFileName(file), LocalPath = file, IsDownload = false }.ToListItem(useLocalPath: true);
            if (!operations.DeleteExcluded && filters != null && !FtpRule.IsAllAllowed(filters, localMetadata)) { continue; }
            transaction.DeleteFile(GetLocalRelativePath(localRoot, file));
        }
        transaction.CommitAsync(options.CancellationToken).GetAwaiter().GetResult();
        return results.Select(result => FromFtpResult("DownloadDirectory", localRoot, result)).ToArray();
    }
}
