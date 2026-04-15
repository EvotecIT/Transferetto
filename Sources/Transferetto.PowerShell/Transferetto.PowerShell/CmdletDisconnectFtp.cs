using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Disconnect-FTP cmdlet.
/// </summary>

[Cmdlet("Disconnect", "FTP")]
public sealed class CmdletDisconnectFtp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(ValueFromPipeline = true)]
	public TransferettoFtpSession? Client { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectFtp(Client);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectFtpFailed", ErrorCategory.CloseError, Client.Host));
		}
	}
}

