using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Clear-SSHShellBuffer cmdlet.
/// </summary>

[Cmdlet("Clear", "SSHShellBuffer")]
public sealed class CmdletClearSshShellBuffer : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			string sendToPipeline = TransferettoClient.ClearSshShellBuffer(ShellSession);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ClearSshShellBufferFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}

