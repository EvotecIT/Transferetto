using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Moves or renames a file on an FTP or FTPS server.</para>
/// <para type="description">Relocates a remote FTP file to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.</para>
/// <example>
///   <para>Rename a file in place.</para>
///   <code>Move-FTPFile -Client $ftp -RemoteSource '/incoming/app.zip' -RemoteDestination '/incoming/app-2026-04-22.zip'</code>
/// </example>
/// <example>
///   <para>Move a file into a release folder and overwrite the destination if it already exists.</para>
///   <code>Move-FTPFile -Client $ftp -RemoteSource '/incoming/site.zip' -RemoteDestination '/releases/current/site.zip' -RemoteExists Overwrite</code>
/// </example>
/// </summary>

[Cmdlet("Move", "FTPFile")]
public sealed class CmdletMoveFtpFile : PSCmdlet
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
			TransferettoClient.MoveFtpFile(Client, RemoteSource!, RemoteDestination!, RemoteExists);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "MoveFtpFileFailed", ErrorCategory.WriteError, RemoteDestination));
		}
	}
}
