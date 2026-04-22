using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Starts a remote SSH port-forwarding tunnel.</para>
/// <para type="description">Requests the SSH server to bind a remote host and port, then forwards traffic back through the SSH session to a target host and port reachable from the client side.</para>
/// <example>
///   <para>Expose a local web application on the remote server.</para>
///   <code>Start-SSHRemoteTunnel -SshClient $ssh -BoundPort 8080 -TargetHost '127.0.0.1' -TargetPort 3000</code>
/// </example>
/// <example>
///   <para>Bind the remote tunnel to a specific interface for controlled access.</para>
///   <code>Start-SSHRemoteTunnel -SshClient $ssh -BoundHost '127.0.0.1' -BoundPort 9000 -TargetHost '127.0.0.1' -TargetPort 9000</code>
/// </example>
/// </summary>

[Cmdlet("Start", "SSHRemoteTunnel")]
public sealed class CmdletStartSshRemoteTunnel : PSCmdlet
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
	/// Gets or sets the target Host.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? TargetHost { get; set; }
	/// <summary>
	/// Gets or sets the target Port.
	/// </summary>

	[Parameter(Mandatory = true)]
	public uint TargetPort { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SshClient == null || string.IsNullOrWhiteSpace(TargetHost))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.StartSshRemoteTunnel(SshClient, BoundPort, TargetHost!, TargetPort, BoundHost));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StartSshRemoteTunnelFailed", ErrorCategory.OpenError, SshClient.Host));
		}
	}
}
