using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto;

public static partial class TransferettoClient {
    /// <summary>
    /// Asynchronously synchronizes a local directory with an FTP or FTPS directory.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoSyncResult>> SyncFtpDirectoryAsync(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        TransferettoSyncOptions? syncOptions = null,
        TransferettoTransferOptions? transferOptions = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedTransferOptions => SyncFtpDirectory(session, localPath, remotePath, syncOptions, resolvedTransferOptions),
            transferOptions,
            cancellationToken);
    }

    /// <summary>
    /// Asynchronously synchronizes a local directory with an SFTP directory.
    /// </summary>
    public static Task<IReadOnlyList<TransferettoSyncResult>> SyncSftpDirectoryAsync(
        TransferettoSftpSession session,
        string localPath,
        string remotePath,
        TransferettoSyncOptions? syncOptions = null,
        TransferettoTransferOptions? transferOptions = null,
        CancellationToken cancellationToken = default) {
        return RunTransferAsync(
            resolvedTransferOptions => SyncSftpDirectory(session, localPath, remotePath, syncOptions, resolvedTransferOptions),
            transferOptions,
            cancellationToken);
    }
}
