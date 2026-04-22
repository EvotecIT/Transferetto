using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Stops a running interactive SSH shell command and waits for the prompt to return.</para>
/// <para type="description">Uses the shell stop lane to interrupt the active command, optionally waiting for a resolved prompt pattern or preset before returning the captured stop result.</para>
/// <example>
///   <para>Stop the current command and wait for a Linux prompt.</para>
///   <code>Stop-SSHShellCommand -ShellSession $shell -PromptPreset Linux</code>
/// </example>
/// <example>
///   <para>Stop the current command with a timeout and limited transcript lookback.</para>
///   <code>Stop-SSHShellCommand -ShellSession $shell -PromptPattern '(?m)^deploy@web01:[^\\r\\n]*\\$\\s?$' -TimeoutSeconds 10 -Lookback 200</code>
/// </example>
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
	/// Gets or sets the reusable prompt preset applied when no explicit prompt pattern is supplied.
	/// </summary>

	[Parameter]
	public TransferettoSshShellPromptPreset PromptPreset { get; set; }
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
			string? promptPattern = TransferettoClient.ResolveSshShellPromptPattern(PromptPattern, PromptPreset);
			WriteObject(TransferettoClient.StopSshShellCommand(ShellSession, timeout, promptPattern, Lookback));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StopSshShellCommandFailed", ErrorCategory.OperationStopped, ShellSession.Host));
		}
	}
}
