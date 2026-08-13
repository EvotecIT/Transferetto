using System;
using System.Collections.Generic;
using System.IO;
using Transferetto.Core;

namespace Transferetto;

internal interface ISftpTransferCommitOperations {
    bool Exists(string path);

    void Delete(string path);

    void Rename(string sourcePath, string destinationPath, bool posix);
}

internal sealed class SftpTransferCommitOperations : ISftpTransferCommitOperations {
    private readonly TransferettoSftpSession _session;

    internal SftpTransferCommitOperations(TransferettoSftpSession session) {
        _session = session;
    }

    public bool Exists(string path) => _session.Client.Exists(path);

    public void Delete(string path) => TransferettoClient.RemoveSftpFile(_session, path);

    public void Rename(string sourcePath, string destinationPath, bool posix) =>
        TransferettoClient.MoveSftpFile(_session, sourcePath, destinationPath, posix);
}

internal static class SftpTransferCommit {
    internal static bool Commit(
        ISftpTransferCommitOperations operations,
        string temporaryPath,
        string destinationPath,
        string relativePath,
        TransferWriteMode mode) {
        if (mode == TransferWriteMode.Overwrite) {
            return CommitOverwrite(operations, temporaryPath, destinationPath);
        }

        if (operations.Exists(destinationPath)) {
            if (mode == TransferWriteMode.SkipIfExists) {
                TryDelete(operations, temporaryPath);
                return false;
            }
            throw new IOException($"The destination SFTP item already exists: {relativePath}");
        }

        try {
            operations.Rename(temporaryPath, destinationPath, posix: false);
            return true;
        } catch (Exception exception) {
            if (!operations.Exists(destinationPath)) {
                throw;
            }
            if (mode == TransferWriteMode.SkipIfExists) {
                TryDelete(operations, temporaryPath);
                return false;
            }
            throw new IOException($"The destination SFTP item already exists: {relativePath}", exception);
        }
    }

    private static bool CommitOverwrite(
        ISftpTransferCommitOperations operations,
        string temporaryPath,
        string destinationPath) {
        try {
            operations.Rename(temporaryPath, destinationPath, posix: true);
            return true;
        } catch (NotSupportedException) {
            // SFTP v3 cannot replace a destination with the base rename operation. Servers that do not
            // advertise the OpenSSH POSIX rename extension need a recoverable move-aside fallback.
        }

        List<string> displacedPaths = new();
        string? rollbackPath = null;
        Exception? commitFailure = null;
        try {
            for (int attempt = 0; attempt < 2; attempt++) {
                if (operations.Exists(destinationPath)) {
                    string displacedPath = ProtocolTransferEndpointPath.CreateTemporaryPath(destinationPath) + ".previous";
                    operations.Rename(destinationPath, displacedPath, posix: false);
                    displacedPaths.Add(displacedPath);
                    rollbackPath ??= displacedPath;
                }

                try {
                    operations.Rename(temporaryPath, destinationPath, posix: false);
                    commitFailure = null;
                    break;
                } catch (Exception exception) {
                    commitFailure = exception;
                    if (!operations.Exists(temporaryPath) && operations.Exists(destinationPath)) {
                        commitFailure = null;
                        break;
                    }
                    if (attempt == 0 && operations.Exists(destinationPath)) {
                        continue;
                    }
                    throw;
                }
            }

            if (commitFailure != null) {
                throw commitFailure;
            }

        } catch (Exception commitException) {
            if (rollbackPath == null) {
                throw;
            }

            try {
                if (operations.Exists(destinationPath) || !operations.Exists(rollbackPath)) {
                    throw new IOException(
                        $"The SFTP overwrite failed and the original item remains at '{rollbackPath}'.",
                        commitException);
                }
                operations.Rename(rollbackPath, destinationPath, posix: false);
                displacedPaths.Remove(rollbackPath);
            } catch (Exception rollbackException) {
                if (rollbackException is IOException && ReferenceEquals(rollbackException.InnerException, commitException)) {
                    throw;
                }
                throw new IOException(
                    $"The SFTP overwrite failed and the original item remains at '{rollbackPath}'.",
                    new AggregateException(commitException, rollbackException));
            }

            List<string> retainedPaths = CleanupDisplacedPaths(operations, displacedPaths);
            if (retainedPaths.Count > 0) {
                throw new IOException(
                    "The SFTP overwrite failed and the original item was restored, but displaced item cleanup failed. " +
                    $"Retained item(s): {string.Join(", ", retainedPaths)}",
                    commitException);
            }
            throw;
        }

        List<string> committedRetainedPaths = CleanupDisplacedPaths(operations, displacedPaths);
        if (committedRetainedPaths.Count > 0) {
            throw new IOException(
                "The SFTP overwrite committed, but displaced item cleanup failed. " +
                $"Retained item(s): {string.Join(", ", committedRetainedPaths)}");
        }
        return true;
    }

    private static List<string> CleanupDisplacedPaths(
        ISftpTransferCommitOperations operations,
        IEnumerable<string> paths) {
        List<string> retainedPaths = new();
        foreach (string path in paths) {
            try {
                if (operations.Exists(path)) {
                    operations.Delete(path);
                }
            } catch {
                // Verify the final state below. A server may commit a delete even when its response is lost.
            }

            try {
                if (operations.Exists(path)) {
                    retainedPaths.Add(path);
                }
            } catch {
                retainedPaths.Add(path);
            }
        }
        return retainedPaths;
    }

    private static void TryDelete(ISftpTransferCommitOperations operations, string path) {
        try {
            if (operations.Exists(path)) {
                operations.Delete(path);
            }
        } catch {
            // Cleanup must not obscure the transfer outcome or the recoverable backup location.
        }
    }
}
