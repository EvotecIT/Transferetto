using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SSHCommand cmdlet.
/// </summary>

[Cmdlet("Send", "SSHCommand")]
public sealed class CmdletSendSshCommand : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshSession? SshClient { get; set; }
	/// <summary>
	/// Gets or sets the command.
	/// </summary>

	[Parameter]
	public ScriptBlock? Command { get; set; }
	/// <summary>
	/// Gets or sets the status.
	/// </summary>

	[Parameter]
	public SwitchParameter Status { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SshClient == null || Command == null)
		{
			return;
		}
		try
		{
			IEnumerable<string> commands = (from output in Command.Invoke()
				select output?.ToString() into value
				where !string.IsNullOrWhiteSpace(value)
				select value).Cast<string>();
			TransferettoSshCommandResult transferettoSshCommandResult = TransferettoClient.SendSshCommand(SshClient, commands);
			if (Status.IsPresent)
			{
				WriteObject(transferettoSshCommandResult);
			}
			else
			{
				WriteObject(transferettoSshCommandResult.Output);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSshCommandFailed", ErrorCategory.WriteError, SshClient.Host));
		}
	}
}

