using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Stops an SSH tunnel session.</para>
/// <para type="description">Closes a tunnel created by <c>Start-SSHLocalTunnel</c> or <c>Start-SSHRemoteTunnel</c>, releasing the forwarded port cleanly.</para>
/// <example>
///   <para>Stop an active SSH tunnel when forwarding is no longer needed.</para>
///   <code>Stop-SSHTunnel -TunnelSession $tunnel</code>
/// </example>
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
