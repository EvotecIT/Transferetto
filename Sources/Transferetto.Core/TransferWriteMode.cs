namespace Transferetto.Core;

/// <summary>
/// Defines how a destination handles an item that already exists.
/// </summary>
public enum TransferWriteMode {
    /// <summary>Return the existing item without replacing its content.</summary>
    SkipIfExists,
    /// <summary>Fail when the destination item already exists.</summary>
    FailIfExists,
    /// <summary>Replace the destination item.</summary>
    Overwrite
}
