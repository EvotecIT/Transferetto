namespace Transferetto;

/// <summary>
/// Defines how existing source and destination files are compared.
/// </summary>
public enum TransferettoSyncComparison {
    /// <summary>
    /// Transfer every included source file.
    /// </summary>
    Always,

    /// <summary>
    /// Transfer only when file sizes differ.
    /// </summary>
    Size,

    /// <summary>
    /// Transfer only when last-write timestamps differ outside the configured tolerance.
    /// </summary>
    LastWriteTime,

    /// <summary>
    /// Transfer when either file size or last-write timestamp differs.
    /// </summary>
    SizeOrLastWriteTime
}
