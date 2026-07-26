namespace Transferetto.Core;

/// <summary>
/// Identifies how a transfer request settled.
/// </summary>
public enum TransferReceiptOutcome {
    /// <summary>Content was streamed to the destination.</summary>
    Copied,
    /// <summary>The destination item already existed and collision policy selected it.</summary>
    Skipped
}
