using System;

namespace Transferetto;
/// <summary>
/// Represents a chunk of progressive output from a non-interactive SSH command.
/// </summary>

public sealed class TransferettoSshCommandOutputChunk {
    /// <summary>
    /// Gets or sets the output stream that produced the chunk.
    /// </summary>
    public TransferettoSshCommandOutputStream Stream { get; init; }
    /// <summary>
    /// Gets or sets the text that was emitted.
    /// </summary>

    public string Text { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the UTC timestamp when the chunk was observed.
    /// </summary>

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
