using System;

namespace Transferetto;
/// <summary>
/// Represents one captured SSH shell transcript entry.
/// </summary>

public sealed class TransferettoSshShellTranscriptEntry {
    /// <summary>
    /// Gets or sets the timestamp Utc.
    /// </summary>
    public DateTime TimestampUtc { get; init; }
    /// <summary>
    /// Gets or sets the direction.
    /// </summary>

    public TransferettoSshShellTranscriptDirection Direction { get; init; }
    /// <summary>
    /// Gets or sets the text.
    /// </summary>

    public string Text { get; init; } = string.Empty;
}
