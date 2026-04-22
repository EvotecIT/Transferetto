using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Disconnects an SSH session.</para>
/// <para type="description">Closes the reusable SSH session created by <c>Connect-SSH</c>, ending command, shell, and tunnel activity tied to that session when shutdown is complete.</para>
/// <example>
///   <para>Close the SSH session after finishing command work.</para>
///   <code>Disconnect-SSH -SshClient $ssh</code>
/// </example>
/// </summary>

[Cmdlet("Disconnect", "SSH")]
public sealed class CmdletDisconnectSsh : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshSession? SshClient { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SshClient == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectSsh(SshClient);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectSshFailed", ErrorCategory.CloseError, SshClient.Host));
		}
	}
}
