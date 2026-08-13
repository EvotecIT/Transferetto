using System;

namespace Transferetto;
/// <summary>
/// Represents a chunk of progressive output from an interactive SSH shell session.
/// </summary>

public sealed class TransferettoSshShellOutputChunk {
    /// <summary>
    /// Gets or sets the text that was emitted by the shell.
    /// </summary>
    public string Text { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the UTC timestamp when the chunk was observed.
    /// </summary>

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
