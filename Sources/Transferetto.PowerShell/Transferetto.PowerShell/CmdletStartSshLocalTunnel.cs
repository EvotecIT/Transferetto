using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Start-SSHLocalTunnel cmdlet.
/// </summary>

[Cmdlet("Start", "SSHLocalTunnel")]
public sealed class CmdletStartSshLocalTunnel : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshSession? SshClient { get; set; }
	/// <summary>
	/// Gets or sets the bound Host.
	/// </summary>

	[Parameter]
	public string BoundHost { get; set; } = "127.0.0.1";
	/// <summary>
	/// Gets or sets the bound Port.
	/// </summary>

	[Parameter(Mandatory = true)]
	public uint BoundPort { get; set; }
	/// <summary>
	/// Gets or sets the remote Host.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemoteHost { get; set; }
	/// <summary>
	/// Gets or sets the remote Port.
	/// </summary>

	[Parameter(Mandatory = true)]
	public uint RemotePort { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SshClient == null || string.IsNullOrWhiteSpace(RemoteHost))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.StartSshLocalTunnel(SshClient, BoundPort, RemoteHost!, RemotePort, BoundHost));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StartSshLocalTunnelFailed", ErrorCategory.OpenError, SshClient.Host));
		}
	}
}

