using System;
using System.Collections.Generic;

namespace Transferetto;
/// <summary>
/// Represents a bounded snapshot of the SSH shell transcript buffer.
/// </summary>

public sealed class TransferettoSshShellTranscriptSnapshot {
    /// <summary>
    /// Gets or sets the transcript entries captured in the snapshot.
    /// </summary>
    public IReadOnlyList<TransferettoSshShellTranscriptEntry> Entries { get; init; } = Array.Empty<TransferettoSshShellTranscriptEntry>();
    /// <summary>
    /// Gets or sets the captured Entry Count.
    /// </summary>

    public int CapturedEntryCount { get; init; }
    /// <summary>
    /// Gets or sets the dropped Entry Count.
    /// </summary>

    public int DroppedEntryCount { get; init; }
    /// <summary>
    /// Gets or sets the total Character Count.
    /// </summary>

    public int TotalCharacterCount { get; init; }
    /// <summary>
    /// Gets a value indicating whether truncated.
    /// </summary>

    public bool IsTruncated => DroppedEntryCount > 0;
}
