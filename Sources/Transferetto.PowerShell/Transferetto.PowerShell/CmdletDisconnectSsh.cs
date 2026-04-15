using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Disconnect-SSH cmdlet.
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

