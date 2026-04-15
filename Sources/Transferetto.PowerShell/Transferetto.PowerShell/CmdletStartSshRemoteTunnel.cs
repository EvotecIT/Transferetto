using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Start-SSHRemoteTunnel cmdlet.
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

