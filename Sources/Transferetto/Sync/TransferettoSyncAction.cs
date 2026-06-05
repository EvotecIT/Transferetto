namespace Transferetto;

/// <summary>
/// Represents a single synchronization operation.
/// </summary>
public enum TransferettoSyncAction {
    /// <summary>
    /// Creates a missing local or remote directory.
    /// </summary>
    CreateDirectory,

    /// <summary>
    /// Uploads a local file to the remote path.
    /// </summary>
    UploadFile,

    /// <summary>
    /// Downloads a remote file to the local path.
    /// </summary>
    DownloadFile,

    /// <summary>
    /// Removes a destination file from the remote path during mirror synchronization.
    /// </summary>
    DeleteRemoteFile,

    /// <summary>
    /// Removes a destination directory from the remote path during mirror synchronization.
    /// </summary>
    DeleteRemoteDirectory,

    /// <summary>
    /// Removes a destination local file during mirror synchronization.
    /// </summary>
    DeleteLocalFile,

    /// <summary>
    /// Removes a destination local directory during mirror synchronization.
    /// </summary>
    DeleteLocalDirectory,

    /// <summary>
    /// Records that no transfer is needed for an included file or directory.
    /// </summary>
    Skip
}
