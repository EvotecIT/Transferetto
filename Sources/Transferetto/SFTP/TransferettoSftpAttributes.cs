using System;
using Renci.SshNet.Sftp;

namespace Transferetto;
/// <summary>
/// Represents SFTP file system metadata and permission details.
/// </summary>

public sealed class TransferettoSftpAttributes {
    /// <summary>
    /// Gets or sets the path associated with the operation.
    /// </summary>
    public string Path { get; set; } = string.Empty;
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
    /// Gets or sets the size.
    /// </summary>

    public long Size { get; set; }
    /// <summary>
    /// Gets or sets the last Access Time.
    /// </summary>

    public DateTime LastAccessTime { get; set; }
    /// <summary>
    /// Gets or sets the last Access Time Utc.
    /// </summary>

    public DateTime LastAccessTimeUtc { get; set; }
    /// <summary>
    /// Gets or sets the last Write Time.
    /// </summary>

    public DateTime LastWriteTime { get; set; }
    /// <summary>
    /// Gets or sets the last Write Time Utc.
    /// </summary>

    public DateTime LastWriteTimeUtc { get; set; }
    /// <summary>
    /// Gets or sets the user Id.
    /// </summary>

    public int UserId { get; set; }
    /// <summary>
    /// Gets or sets the group Id.
    /// </summary>

    public int GroupId { get; set; }
    /// <summary>
    /// Gets or sets the permissions Value.
    /// </summary>

    public short PermissionsValue { get; set; }
    /// <summary>
    /// Gets or sets the permissions Octal.
    /// </summary>

    public string PermissionsOctal { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the owner Can Read.
    /// </summary>

    public bool OwnerCanRead { get; set; }
    /// <summary>
    /// Gets or sets the owner Can Write.
    /// </summary>

    public bool OwnerCanWrite { get; set; }
    /// <summary>
    /// Gets or sets the owner Can Execute.
    /// </summary>

    public bool OwnerCanExecute { get; set; }
    /// <summary>
    /// Gets or sets the group Can Read.
    /// </summary>

    public bool GroupCanRead { get; set; }
    /// <summary>
    /// Gets or sets the group Can Write.
    /// </summary>

    public bool GroupCanWrite { get; set; }
    /// <summary>
    /// Gets or sets the group Can Execute.
    /// </summary>

    public bool GroupCanExecute { get; set; }
    /// <summary>
    /// Gets or sets the others Can Read.
    /// </summary>

    public bool OthersCanRead { get; set; }
    /// <summary>
    /// Gets or sets the others Can Write.
    /// </summary>

    public bool OthersCanWrite { get; set; }
    /// <summary>
    /// Gets or sets the others Can Execute.
    /// </summary>

    public bool OthersCanExecute { get; set; }

    internal static TransferettoSftpAttributes FromFileAttributes(string path, SftpFileAttributes attributes) {
        short permissionsValue = CalculatePermissionsValue(attributes);
        return new TransferettoSftpAttributes {
            Path = path,
            IsDirectory = attributes.IsDirectory,
            IsRegularFile = attributes.IsRegularFile,
            IsSymbolicLink = attributes.IsSymbolicLink,
            Size = attributes.Size,
            LastAccessTime = attributes.LastAccessTime,
            LastAccessTimeUtc = attributes.LastAccessTimeUtc,
            LastWriteTime = attributes.LastWriteTime,
            LastWriteTimeUtc = attributes.LastWriteTimeUtc,
            UserId = attributes.UserId,
            GroupId = attributes.GroupId,
            PermissionsValue = permissionsValue,
            PermissionsOctal = Convert.ToString(permissionsValue, 8).PadLeft(3, '0'),
            OwnerCanRead = attributes.OwnerCanRead,
            OwnerCanWrite = attributes.OwnerCanWrite,
            OwnerCanExecute = attributes.OwnerCanExecute,
            GroupCanRead = attributes.GroupCanRead,
            GroupCanWrite = attributes.GroupCanWrite,
            GroupCanExecute = attributes.GroupCanExecute,
            OthersCanRead = attributes.OthersCanRead,
            OthersCanWrite = attributes.OthersCanWrite,
            OthersCanExecute = attributes.OthersCanExecute
        };
    }

    private static short CalculatePermissionsValue(SftpFileAttributes attributes) {
        int owner = (attributes.OwnerCanRead ? 4 : 0)
            + (attributes.OwnerCanWrite ? 2 : 0)
            + (attributes.OwnerCanExecute ? 1 : 0);
        int group = (attributes.GroupCanRead ? 4 : 0)
            + (attributes.GroupCanWrite ? 2 : 0)
            + (attributes.GroupCanExecute ? 1 : 0);
        int other = (attributes.OthersCanRead ? 4 : 0)
            + (attributes.OthersCanWrite ? 2 : 0)
            + (attributes.OthersCanExecute ? 1 : 0);
        string octal = $"{owner}{group}{other}";
        return Convert.ToInt16(octal, 8);
    }
}
