namespace Transferetto;
/// <summary>
/// Represents the outcome of a file or directory transfer.
/// </summary>

public sealed class TransferettoTransferResult {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the operation completed successfully.
    /// </summary>

    public bool IsSuccess { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the operation was skipped.
    /// </summary>

    public bool IsSkipped { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the operation was skipped by a transfer rule.
    /// </summary>

    public bool IsSkippedByRule { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the operation failed.
    /// </summary>

    public bool IsFailed { get; init; }
    /// <summary>
    /// Gets or sets the local path.
    /// </summary>

    public string? LocalPath { get; init; }
    /// <summary>
    /// Gets or sets the remote path.
    /// </summary>

    public string? RemotePath { get; init; }
    /// <summary>
    /// Gets or sets the message reported for the operation.
    /// </summary>

    public string Message { get; init; } = string.Empty;
}
