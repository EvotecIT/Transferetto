using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Stop-SSHTunnel cmdlet.
/// </summary>

[Cmdlet("Stop", "SSHTunnel")]
public sealed class CmdletStopSshTunnel : PSCmdlet
{
	/// <summary>
	/// Gets or sets the tunnel Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshTunnelSession? TunnelSession { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (TunnelSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.StopSshTunnel(TunnelSession);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StopSshTunnelFailed", ErrorCategory.CloseError, TunnelSession.Host));
		}
	}
}

