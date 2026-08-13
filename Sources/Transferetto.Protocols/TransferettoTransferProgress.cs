using System;

namespace Transferetto;
/// <summary>
/// Represents progress information reported during a transfer.
/// </summary>

public sealed class TransferettoTransferProgress {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the protocol name associated with the operation.
    /// </summary>

    public string Protocol { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the transfer direction.
    /// </summary>

    public TransferettoTransferDirection Direction { get; init; }
    /// <summary>
    /// Gets or sets the local path.
    /// </summary>

    public string? LocalPath { get; init; }
    /// <summary>
    /// Gets or sets the remote path.
    /// </summary>

    public string? RemotePath { get; init; }
    /// <summary>
    /// Gets or sets the number of bytes transferred so far.
    /// </summary>

    public long BytesTransferred { get; init; }
    /// <summary>
    /// Gets or sets the expected total number of bytes when known.
    /// </summary>

    public long? TotalBytes { get; init; }
    /// <summary>
    /// Gets the transfer completion percentage when the total size is known.
    /// </summary>

    public int? PercentComplete => TotalBytes.HasValue && TotalBytes.Value > 0
        ? (int) Math.Min(100, Math.Round(BytesTransferred * 100.0 / TotalBytes.Value, MidpointRounding.AwayFromZero))
        : null;
}
