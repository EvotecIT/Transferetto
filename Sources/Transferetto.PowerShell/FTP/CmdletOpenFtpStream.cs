using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Open-FTPStream cmdlet.
/// </summary>

[Cmdlet("Open", "FTPStream")]
public sealed class CmdletOpenFtpStream : PSCmdlet
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
	/// Gets or sets the mode.
	/// </summary>

	[Parameter]
	public TransferettoFtpStreamMode Mode { get; set; } = TransferettoFtpStreamMode.Read;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.OpenFtpStream(Client, RemotePath!, Mode));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "OpenFtpStreamFailed", ErrorCategory.OpenError, RemotePath));
		}
	}
}

