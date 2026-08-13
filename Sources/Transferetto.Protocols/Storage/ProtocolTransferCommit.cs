using System;
using System.Collections.Generic;
using System.IO;
using Transferetto.Core;

namespace Transferetto;

internal interface IProtocolTransferCommitOperations {
    bool Exists(string path);

    void Delete(string path);

    void Rename(string sourcePath, string destinationPath, bool overwriteAtomically);
}

internal static class ProtocolTransferCommit {
    internal static bool Commit(
        IProtocolTransferCommitOperations operations,
        string temporaryPath,
        string destinationPath,
        string relativePath,
        TransferWriteMode mode,
        string scheme) {
        if (mode == TransferWriteMode.Overwrite) {
            return CommitOverwrite(operations, temporaryPath, destinationPath, scheme);
        }

        if (operations.Exists(destinationPath)) {
            if (mode == TransferWriteMode.SkipIfExists) {
                DeleteTemporaryOrThrow(operations, temporaryPath, scheme);
                return false;
            }
            throw new IOException($"The destination {scheme} item already exists: {relativePath}");
        }

        try {
            RenameVerified(operations, temporaryPath, destinationPath, overwriteAtomically: false);
            return true;
        } catch (Exception exception) {
            if (!operations.Exists(destinationPath)) {
                throw;
            }
            if (mode == TransferWriteMode.SkipIfExists) {
                DeleteTemporaryOrThrow(operations, temporaryPath, scheme);
                return false;
            }
            throw new IOException($"The destination {scheme} item already exists: {relativePath}", exception);
        }
    }

    private static bool CommitOverwrite(
        IProtocolTransferCommitOperations operations,
        string temporaryPath,
        string destinationPath,
        string scheme) {
        try {
            RenameVerified(operations, temporaryPath, destinationPath, overwriteAtomically: true);
            return true;
        } catch (NotSupportedException) {
            // Protocols without a portable atomic replacement use the recoverable move-aside path below.
        }

        List<string> displacedPaths = new();
        string? rollbackPath = null;
        Exception? commitFailure = null;
        try {
            for (int attempt = 0; attempt < 2; attempt++) {
                if (operations.Exists(destinationPath)) {
                    string displacedPath = ProtocolTransferEndpointPath.CreateTemporaryPath(destinationPath) + ".previous";
                    RenameVerified(
                        operations,
                        destinationPath,
                        displacedPath,
                        overwriteAtomically: false,
                        destinationProvesSuccess: true);
                    displacedPaths.Add(displacedPath);
                    rollbackPath = displacedPath;
                }

                try {
                    RenameVerified(operations, temporaryPath, destinationPath, overwriteAtomically: false);
                    commitFailure = null;
                    break;
                } catch (Exception exception) {
                    commitFailure = exception;
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

            Exception? rollbackFailure = null;
            try {
                if (operations.Exists(destinationPath) || !operations.Exists(rollbackPath)) {
                    throw new IOException($"The latest displaced {scheme} item could not be restored.");
                }
                RenameVerified(operations, rollbackPath, destinationPath, overwriteAtomically: false);
                displacedPaths.Remove(rollbackPath);
            } catch (Exception rollbackException) {
                rollbackFailure = rollbackException;
            }

            if (rollbackFailure != null) {
                List<string> unrecoveredPaths = GetRetainedPaths(operations, displacedPaths);
                throw new IOException(
                    $"The {scheme} overwrite failed and displaced item recovery also failed. " +
                    $"Retained item(s): {string.Join(", ", unrecoveredPaths)}",
                    new AggregateException(commitException, rollbackFailure));
            }

            List<string> retainedPaths = CleanupDisplacedPaths(operations, displacedPaths);
            if (retainedPaths.Count > 0) {
                throw new IOException(
                    $"The {scheme} overwrite failed and the original item was restored, but displaced item cleanup failed. " +
                    $"Retained item(s): {string.Join(", ", retainedPaths)}",
                    commitException);
            }
            throw;
        }

        List<string> committedRetainedPaths = CleanupDisplacedPaths(operations, displacedPaths);
        if (committedRetainedPaths.Count > 0) {
            throw new IOException(
                $"The {scheme} overwrite committed, but displaced item cleanup failed. " +
                $"Retained item(s): {string.Join(", ", committedRetainedPaths)}");
        }
        return true;
    }

    private static void RenameVerified(
        IProtocolTransferCommitOperations operations,
        string sourcePath,
        string destinationPath,
        bool overwriteAtomically,
        bool destinationProvesSuccess = false) {
        try {
            operations.Rename(sourcePath, destinationPath, overwriteAtomically);
        } catch {
            bool sourceExists = operations.Exists(sourcePath);
            bool destinationExists = operations.Exists(destinationPath);
            if (destinationExists && (!sourceExists || destinationProvesSuccess)) {
                return;
            }
            throw;
        }
    }

    private static List<string> GetRetainedPaths(
        IProtocolTransferCommitOperations operations,
        IEnumerable<string> paths) {
        List<string> retainedPaths = new();
        foreach (string path in paths) {
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

    private static List<string> CleanupDisplacedPaths(
        IProtocolTransferCommitOperations operations,
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

    private static void DeleteTemporaryOrThrow(
        IProtocolTransferCommitOperations operations,
        string path,
        string scheme) {
        Exception? deleteFailure = null;
        try {
            if (operations.Exists(path)) {
                operations.Delete(path);
            }
        } catch (Exception exception) {
            deleteFailure = exception;
        }

        try {
            if (!operations.Exists(path)) {
                return;
            }
        } catch (Exception verificationFailure) {
            throw new IOException(
                $"The {scheme} transfer was skipped, but temporary item cleanup could not be verified: '{path}'.",
                deleteFailure == null
                    ? verificationFailure
                    : new AggregateException(deleteFailure, verificationFailure));
        }

        throw new IOException(
            $"The {scheme} transfer was skipped, but the temporary item was retained at '{path}'.",
            deleteFailure);
    }
}
