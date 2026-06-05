namespace Transferetto;

/// <summary>
/// Defines which side is the source for a directory synchronization.
/// </summary>
public enum TransferettoSyncDirection {
    /// <summary>
    /// Copies local files and folders to the remote FTP or SFTP path.
    /// </summary>
    Upload,

    /// <summary>
    /// Copies remote FTP or SFTP files and folders to the local path.
    /// </summary>
    Download
}
