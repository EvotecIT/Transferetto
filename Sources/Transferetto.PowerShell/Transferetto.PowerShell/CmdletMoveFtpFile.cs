using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Move-FTPFile cmdlet.
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

