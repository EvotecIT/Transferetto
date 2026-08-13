using System;

namespace Transferetto;

/// <summary>
/// Describes one local or remote item in a synchronization manifest.
/// </summary>
public sealed class TransferettoSyncEntry {
    /// <summary>
    /// Gets or sets the path relative to the synchronized root.
    /// </summary>
    public string RelativePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the local filesystem path for this item when known.
    /// </summary>
    public string? LocalPath { get; init; }

    /// <summary>
    /// Gets or sets the remote FTP or SFTP path for this item when known.
    /// </summary>
    public string? RemotePath { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is a directory.
    /// </summary>
    public bool IsDirectory { get; init; }

    /// <summary>
    /// Gets or sets the file length when this item is a file and the length is known.
    /// </summary>
    public long? Length { get; init; }

    /// <summary>
    /// Gets or sets the UTC last-write timestamp when known.
    /// </summary>
    public DateTime? LastWriteTimeUtc { get; init; }
}
