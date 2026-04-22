using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Moves or renames a directory on an FTP or FTPS server.</para>
/// <para type="description">Relocates a remote FTP directory to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.</para>
/// <example>
///   <para>Rename a directory in place.</para>
///   <code>Move-FTPDirectory -Client $ftp -RemoteSource '/incoming/site-next' -RemoteDestination '/incoming/site-current'</code>
/// </example>
/// </summary>

[Cmdlet("Move", "FTPDirectory")]
public sealed class CmdletMoveFtpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Source.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemoteSource { get; set; }
	/// <summary>
	/// Gets or sets the remote Destination.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemoteDestination { get; set; }
	/// <summary>
	/// Gets or sets the remote Exists.
	/// </summary>

	[Parameter]
	public FtpRemoteExists RemoteExists { get; set; } = FtpRemoteExists.Skip;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemoteSource) || string.IsNullOrWhiteSpace(RemoteDestination))
		{
			return;
		}
		try
		{
			TransferettoClient.MoveFtpDirectory(Client, RemoteSource!, RemoteDestination!, RemoteExists);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "MoveFtpDirectoryFailed", ErrorCategory.WriteError, RemoteDestination));
		}
	}
}
