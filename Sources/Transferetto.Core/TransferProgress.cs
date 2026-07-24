using System;

namespace Transferetto.Core;

/// <summary>
/// Reports provider-neutral transfer progress.
/// </summary>
public sealed class TransferProgress {
    /// <summary>Gets the source item path.</summary>
    public string SourcePath { get; init; } = string.Empty;

    /// <summary>Gets the destination item path.</summary>
    public string DestinationPath { get; init; } = string.Empty;

    /// <summary>Gets the number of bytes read from the source.</summary>
    public long BytesTransferred { get; init; }

    /// <summary>Gets the expected total length when known.</summary>
    public long? TotalBytes { get; init; }

    /// <summary>Gets the completion percentage when total length is known.</summary>
    public int? PercentComplete => TotalBytes.HasValue && TotalBytes.Value > 0
        ? (int) Math.Min(100, Math.Round(BytesTransferred * 100.0 / TotalBytes.Value, MidpointRounding.AwayFromZero))
        : null;
}
