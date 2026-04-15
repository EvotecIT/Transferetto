using System;
using FluentFTP;

namespace Transferetto;
/// <summary>
/// Represents a remote FTP or FTPS item returned by Transferetto.
/// </summary>

public sealed class TransferettoRemoteItem {
    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the full path of the remote item.
    /// </summary>

    public string FullName { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the type.
    /// </summary>

    public FtpObjectType Type { get; init; }
    /// <summary>
    /// Gets or sets the modified.
    /// </summary>

    public DateTime Modified { get; init; }
    /// <summary>
    /// Gets or sets the created.
    /// </summary>

    public DateTime Created { get; init; }
    /// <summary>
    /// Gets or sets the size.
    /// </summary>

    public long Size { get; init; }
    /// <summary>
    /// Gets or sets the link Target.
    /// </summary>

    public string? LinkTarget { get; init; }
    /// <summary>
    /// Gets or sets the raw Permissions.
    /// </summary>

    public string? RawPermissions { get; init; }

    internal static TransferettoRemoteItem FromFtpListItem(FtpListItem item) {
        return new TransferettoRemoteItem {
            Name = item.Name,
            FullName = item.FullName,
            Type = item.Type,
            Modified = item.Modified,
            Created = item.Created,
            Size = item.Size,
            LinkTarget = item.LinkTarget,
            RawPermissions = item.RawPermissions
        };
    }
}
