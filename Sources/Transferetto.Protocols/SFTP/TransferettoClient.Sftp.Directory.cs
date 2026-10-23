using System;
using System.Collections.Generic;
using System.IO;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>Receives a frozen SFTP manifest through the listing and file transport boundaries.</summary>
    internal static IReadOnlyList<TransferettoTransferResult> DownloadSftpDirectoryCore(
        Func<string, IReadOnlyList<TransferettoSftpItem>> list,
        Func<string, string, string, TransferettoTransferOptions, TransferettoTransferResult> receive,
        string remoteRoot, string localRoot, bool allowOverride, TransferettoTransferOptions options) {
        var manifest = DirectoryDownloadManifest.Build(remoteRoot, localRoot, list,
            item => item.Name, item => item.IsDirectory && !item.IsSymbolicLink,
            item => item.IsRegularFile && !item.IsSymbolicLink, options.CancellationToken);
        List<TransferettoTransferResult> results = new() { CreateDirectoryTransferResult(localRoot, remoteRoot) };
        using LocalDirectoryTransaction transaction = new(localRoot, "sftp");
        transaction.AddDirectory("", null, null);
        foreach (var member in manifest) {
            options.CancellationToken.ThrowIfCancellationRequested();
            if (member.IsDirectory) {
                transaction.AddDirectory(member.Relative, null, null);
                results.Add(CreateDirectoryTransferResult(member.Local, member.Remote));
            } else if (!allowOverride && File.Exists(member.Local)) {
                results.Add(new TransferettoTransferResult { Action = "DownloadFile", Status = true, IsSuccess = true,
                    IsSkipped = true, LocalPath = member.Local, RemotePath = member.Remote, Message = "Skipped" });
            } else {
                TransferettoTransferResult? result = null;
                bool received = transaction.AddFile(member.Relative, staged => {
                    result = receive(member.Remote, staged, member.Local, options);
                    options.CancellationToken.ThrowIfCancellationRequested();
                    return result.IsSuccess && !result.IsFailed;
                });
                if (!received) { throw new IOException("The SFTP directory file was not received successfully: " + member.Remote); }
                results.Add(result!);
            }
        }
        transaction.CommitAsync(options.CancellationToken).GetAwaiter().GetResult();
        return results;
    }
}
