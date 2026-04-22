using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Invoke-SSHShellCommand cmdlet.
/// </summary>

[Cmdlet("Invoke", "SSHShellCommand")]
public sealed class CmdletInvokeSshShellCommand : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the command.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Command { get; set; }
	/// <summary>
	/// Gets or sets the prompt Pattern.
	/// </summary>

	[Parameter]
	public string? PromptPattern { get; set; }
	/// <summary>
	/// Gets or sets the lookback.
	/// </summary>

	[Parameter]
	public int Lookback { get; set; } = -1;
	/// <summary>
	/// Gets or sets the timeout Seconds.
	/// </summary>

	[Parameter]
	public double TimeoutSeconds { get; set; } = -1.0;
	/// <summary>
	/// Gets or sets the raw Output.
	/// </summary>

	[Parameter]
	public SwitchParameter RawOutput { get; set; }
	/// <summary>
	/// Gets or sets the keep Command Echo.
	/// </summary>

	[Parameter]
	public SwitchParameter KeepCommandEcho { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null || string.IsNullOrWhiteSpace(Command))
		{
			return;
		}
		try
		{
			TimeSpan? timeout = (base.MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds") ? new TimeSpan?(TimeSpan.FromSeconds(TimeoutSeconds)) : ((TimeSpan?)null));
			TransferettoSshShellCommandResult transferettoSshShellCommandResult = TransferettoClient.InvokeSshShellCommand(ShellSession, Command!, timeout, PromptPattern!, !KeepCommandEcho.IsPresent, Lookback);
			if (RawOutput.IsPresent)
			{
				WriteObject(transferettoSshShellCommandResult.Output);
			}
			else
			{
				WriteObject(transferettoSshShellCommandResult);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "InvokeSshShellCommandFailed", ErrorCategory.OperationTimeout, ShellSession.Host));
		}
	}
}

