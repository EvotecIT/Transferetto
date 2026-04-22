using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Read-SSHShell cmdlet.
/// </summary>

[Alias(new string[] { "Receive-SSHShell" })]
[Cmdlet("Read", "SSHShell")]
public sealed class CmdletReadSshShell : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the read Line.
	/// </summary>

	[Parameter]
	public SwitchParameter ReadLine { get; set; }
	/// <summary>
	/// Gets or sets the expect Text.
	/// </summary>

	[Parameter]
	public string? ExpectText { get; set; }
	/// <summary>
	/// Gets or sets the regex Pattern.
	/// </summary>

	[Parameter]
	public string? RegexPattern { get; set; }
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
	/// Gets or sets the read Until Idle.
	/// </summary>

	[Parameter]
	public SwitchParameter ReadUntilIdle { get; set; }
	/// <summary>
	/// Gets or sets the idle Timeout Seconds.
	/// </summary>

	[Parameter]
	public double IdleTimeoutSeconds { get; set; } = 0.5;
	/// <summary>
	/// Gets or sets the expect Prompt.
	/// </summary>

	[Parameter]
	public SwitchParameter ExpectPrompt { get; set; }
	/// <summary>
	/// Gets or sets the prompt Pattern.
	/// </summary>

	[Parameter]
	public string? PromptPattern { get; set; }

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
			TimeSpan? idleTimeout = (base.MyInvocation.BoundParameters.ContainsKey("IdleTimeoutSeconds") ? new TimeSpan?(TimeSpan.FromSeconds(IdleTimeoutSeconds)) : ((TimeSpan?)null));
			string sendToPipeline = TransferettoClient.ReadSshShell(ShellSession, timeout, ReadLine.IsPresent, ExpectText, Lookback, RegexPattern, ReadUntilIdle.IsPresent, idleTimeout, ExpectPrompt.IsPresent, PromptPattern!);
			WriteObject(sendToPipeline);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadSshShellFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}

