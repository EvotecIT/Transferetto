using System;

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
    /// Gets or sets the number of bytes transferred when known.
    /// </summary>

    public long? BytesTransferred { get; init; }
    /// <summary>
    /// Gets or sets the expected total number of bytes when known.
    /// </summary>

    public long? TotalBytes { get; init; }
    /// <summary>
    /// Gets or sets the UTC timestamp when the transfer started.
    /// </summary>

    public DateTime? StartedUtc { get; init; }
    /// <summary>
    /// Gets or sets the UTC timestamp when the transfer completed.
    /// </summary>

    public DateTime? CompletedUtc { get; init; }
    /// <summary>
    /// Gets the transfer duration when start and completion timestamps are known.
    /// </summary>

    public TimeSpan? Elapsed => StartedUtc.HasValue && CompletedUtc.HasValue
        ? CompletedUtc.Value - StartedUtc.Value
        : null;
    /// <summary>
    /// Gets or sets the message reported for the operation.
    /// </summary>

    public string Message { get; init; } = string.Empty;
}
