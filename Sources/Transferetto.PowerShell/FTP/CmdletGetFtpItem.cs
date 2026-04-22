using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Retrieves metadata for a single FTP or FTPS file-system item.</para>
/// <para type="description">Returns a single remote item with file or directory metadata, optionally following symbolic links when the remote server exposes them through the FTP listing surface.</para>
/// <example>
///   <para>Inspect metadata for a remote file.</para>
///   <code>Get-FTPItem -Client $ftp -RemotePath '/pub/example/readme.txt'</code>
/// </example>
/// <example>
///   <para>Resolve a link target when supported by the server.</para>
///   <code>Get-FTPItem -Client $ftp -RemotePath '/pub/current' -FollowLinks</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPItem")]
public sealed class CmdletGetFtpItem : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the follow Links.
	/// </summary>

	[Parameter]
	public SwitchParameter FollowLinks { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpItem(Client, RemotePath!, FollowLinks.IsPresent));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpItemFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
