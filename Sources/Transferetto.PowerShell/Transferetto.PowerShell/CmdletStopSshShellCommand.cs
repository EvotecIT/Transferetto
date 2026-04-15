using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Stop-SSHShellCommand cmdlet.
/// </summary>

[Cmdlet("Stop", "SSHShellCommand")]
public sealed class CmdletStopSshShellCommand : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TimeSpan? timeout = (base.MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds") ? new TimeSpan?(TimeSpan.FromSeconds(TimeoutSeconds)) : ((TimeSpan?)null));
			WriteObject(TransferettoClient.StopSshShellCommand(ShellSession, timeout, PromptPattern!, Lookback));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StopSshShellCommandFailed", ErrorCategory.OperationStopped, ShellSession.Host));
		}
	}
}

