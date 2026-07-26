using System;

namespace Transferetto.Core;

/// <summary>
/// Describes whether a destination write created content or reused an existing item.
/// </summary>
public sealed class TransferWriteResult {
    /// <summary>Initializes a write result.</summary>
    public TransferWriteResult(TransferItem item, bool wasWritten) {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        WasWritten = wasWritten;
    }

    /// <summary>Gets the resulting destination item.</summary>
    public TransferItem Item { get; }

    /// <summary>Gets whether source content was consumed and written.</summary>
    public bool WasWritten { get; }
}
