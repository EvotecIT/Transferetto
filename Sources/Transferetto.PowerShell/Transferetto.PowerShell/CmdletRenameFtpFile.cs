using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Rename-FTPFile cmdlet.
/// </summary>

[Cmdlet("Rename", "FTPFile")]
public sealed class CmdletRenameFtpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(Path) || string.IsNullOrWhiteSpace(DestinationPath))
		{
			return;
		}
		try
		{
			TransferettoClient.RenameFtpFile(Client, Path!, DestinationPath!);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RenameFtpFileFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}

