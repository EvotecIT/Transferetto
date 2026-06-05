using System;

namespace Transferetto;

/// <summary>
/// Configures how Transferetto plans and executes a directory synchronization.
/// </summary>
public sealed class TransferettoSyncOptions {
    /// <summary>
    /// Gets or sets which side is treated as the source.
    /// </summary>
    public TransferettoSyncDirection Direction { get; set; } = TransferettoSyncDirection.Upload;

    /// <summary>
    /// Gets or sets whether synchronization updates only missing/changed items or mirrors deletes too.
    /// </summary>
    public TransferettoSyncMode Mode { get; set; } = TransferettoSyncMode.Update;

    /// <summary>
    /// Gets or sets how existing files are compared before transfer.
    /// </summary>
    public TransferettoSyncComparison Comparison { get; set; } = TransferettoSyncComparison.SizeOrLastWriteTime;

    /// <summary>
    /// Gets or sets a value indicating whether operations are planned but not executed.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether changed existing files may be overwritten.
    /// </summary>
    public bool OverwriteExisting { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether missing destination directories should be created.
    /// </summary>
    public bool CreateDestinationDirectories { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether transferred file timestamps should be preserved where the protocol supports it.
    /// </summary>
    public bool PreserveTimestamps { get; set; } = true;

    /// <summary>
    /// Gets or sets the timestamp tolerance used when comparing last-write times.
    /// </summary>
    public TimeSpan TimestampTolerance { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets wildcard patterns for relative paths that should be included. Empty means include all paths.
    /// </summary>
    public string[]? IncludePatterns { get; set; }

    /// <summary>
    /// Gets or sets wildcard patterns for relative paths that should be excluded.
    /// </summary>
    public string[]? ExcludePatterns { get; set; }
}
