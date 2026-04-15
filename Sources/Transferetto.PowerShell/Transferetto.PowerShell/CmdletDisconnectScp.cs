using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Disconnect-SCP cmdlet.
/// </summary>

[Cmdlet("Disconnect", "SCP")]
public sealed class CmdletDisconnectScp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoScpSession? ScpClient { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ScpClient == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectScp(ScpClient);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectScpFailed", ErrorCategory.CloseError, ScpClient.Host));
		}
	}
}

