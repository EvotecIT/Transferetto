namespace Transferetto;

/// <summary>
/// Defines how aggressively the destination should be brought in line with the source.
/// </summary>
public enum TransferettoSyncMode {
    /// <summary>
    /// Creates missing destination items and updates changed files, leaving extra destination items in place.
    /// </summary>
    Update,

    /// <summary>
    /// Creates and updates destination items, then removes destination items that no longer exist in the source.
    /// </summary>
    Mirror
}
