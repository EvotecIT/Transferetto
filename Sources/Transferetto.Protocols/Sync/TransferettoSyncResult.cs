namespace Transferetto;

/// <summary>
/// Represents the result of one synchronization plan item.
/// </summary>
public sealed class TransferettoSyncResult {
    /// <summary>
    /// Gets or sets the executed or planned action.
    /// </summary>
    public TransferettoSyncAction Action { get; init; }

    /// <summary>
    /// Gets or sets the synchronization direction.
    /// </summary>
    public TransferettoSyncDirection Direction { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the action completed successfully or was valid in dry-run mode.
    /// </summary>
    public bool Status { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this result came from dry-run planning.
    /// </summary>
    public bool IsDryRun { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the item was skipped because no change was needed or overwrite was disabled.
    /// </summary>
    public bool IsSkipped { get; init; }

    /// <summary>
    /// Gets or sets the relative path affected by this result.
    /// </summary>
    public string RelativePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the local filesystem path affected by this result.
    /// </summary>
    public string? LocalPath { get; init; }

    /// <summary>
    /// Gets or sets the remote FTP or SFTP path affected by this result.
    /// </summary>
    public string? RemotePath { get; init; }

    /// <summary>
    /// Gets or sets the number of bytes transferred when known.
    /// </summary>
    public long? BytesTransferred { get; init; }

    /// <summary>
    /// Gets or sets a short message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
