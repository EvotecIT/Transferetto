using System;
using Renci.SshNet.Sftp;

namespace Transferetto;
/// <summary>
/// Represents a remote SFTP item returned by Transferetto.
/// </summary>

public sealed class TransferettoSftpItem {
    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the full path of the remote item.
    /// </summary>

    public string FullName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether directory.
    /// </summary>

    public bool IsDirectory { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether regular File.
    /// </summary>

    public bool IsRegularFile { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether symbolic Link.
    /// </summary>

    public bool IsSymbolicLink { get; set; }
    /// <summary>
    /// Gets or sets the length.
    /// </summary>

    public long Length { get; set; }
    /// <summary>
    /// Gets or sets the last Write Time.
    /// </summary>

    public DateTime LastWriteTime { get; set; }

    internal static TransferettoSftpItem FromSftpFile(ISftpFile file) {
        return new TransferettoSftpItem {
            Name = file.Name,
            FullName = file.FullName,
            IsDirectory = file.IsDirectory,
            IsRegularFile = file.IsRegularFile,
            IsSymbolicLink = file.IsSymbolicLink,
            Length = file.Length,
            LastWriteTime = file.LastWriteTime
        };
    }
}
