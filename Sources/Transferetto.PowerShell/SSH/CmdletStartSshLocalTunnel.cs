using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Starts a local SSH port-forwarding tunnel.</para>
/// <para type="description">Binds a local host and port, then forwards traffic through the SSH session to a remote host and port, returning a reusable tunnel session that can be stopped later.</para>
/// <example>
///   <para>Expose a remote web application on a local port.</para>
///   <code>Start-SSHLocalTunnel -SshClient $ssh -BoundPort 8080 -RemoteHost '127.0.0.1' -RemotePort 80</code>
/// </example>
/// <example>
///   <para>Bind a tunnel to all local interfaces for a shared troubleshooting session.</para>
///   <code>Start-SSHLocalTunnel -SshClient $ssh -BoundHost '0.0.0.0' -BoundPort 15432 -RemoteHost 'db.internal' -RemotePort 5432</code>
/// </example>
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
