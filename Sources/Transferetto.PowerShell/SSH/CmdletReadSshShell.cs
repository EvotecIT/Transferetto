using System;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads output from an interactive SSH shell session.</para>
/// <para type="description">Supports simple reads, line reads, read-until-idle, text and regex expectations, prompt waits, follow-mode output capture, prompt presets, progressive streaming, and cancellation-aware polling for interactive shell automation.</para>
/// <example>
///   <para>Drain the shell output until it becomes idle.</para>
///   <code>Read-SSHShell -ShellSession $shell -ReadUntilIdle</code>
/// </example>
/// <example>
///   <para>Wait until a Linux prompt appears by using the built-in prompt preset.</para>
///   <code>Read-SSHShell -ShellSession $shell -ExpectPrompt -PromptPreset LinuxRoot</code>
/// </example>
/// <example>
///   <para>Follow shell output until a stop pattern is observed and stream chunks to the pipeline while waiting.</para>
///   <code>Read-SSHShell -ShellSession $shell -Follow -RegexPattern 'completed successfully' -StreamOutput -TimeoutSeconds 60</code>
/// </example>
/// </summary>
[Alias(new string[] { "Receive-SSHShell" })]
[Cmdlet("Read", "SSHShell")]
public sealed class CmdletReadSshShell : AsyncPSCmdlet
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
	/// <summary>
	/// Gets or sets the reusable prompt preset applied when no explicit prompt pattern is supplied.
	/// </summary>

	[Parameter]
	public TransferettoSshShellPromptPreset PromptPreset { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether output should be followed until cancellation, timeout, or an optional stop pattern.
	/// </summary>

	[Parameter]
	public SwitchParameter Follow { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether progressive output chunks are written to the pipeline while waiting.
	/// </summary>

	[Parameter]
	public SwitchParameter StreamOutput { get; set; }
	/// <summary>
	/// Gets or sets the poll interval, in milliseconds, used while waiting for shell output.
	/// </summary>

	[Parameter]
	public int PollIntervalMilliseconds { get; set; } = 50;

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TimeSpan? timeout = (base.MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds") ? new TimeSpan?(TimeSpan.FromSeconds(TimeoutSeconds)) : ((TimeSpan?)null));
			TimeSpan? idleTimeout = (base.MyInvocation.BoundParameters.ContainsKey("IdleTimeoutSeconds") ? new TimeSpan?(TimeSpan.FromSeconds(IdleTimeoutSeconds)) : ((TimeSpan?)null));
			string? promptPattern = TransferettoClient.ResolveSshShellPromptPattern(PromptPattern, PromptPreset);
			TransferettoSshShellReadOptions options = new() {
				CancellationToken = CancelToken,
				OutputProgress = StreamOutput.IsPresent ? new TransferettoSshShellOutputProgress(this) : null,
				PollInterval = TimeSpan.FromMilliseconds(PollIntervalMilliseconds > 0 ? PollIntervalMilliseconds : 50)
			};
			string sendToPipeline;
			if (Follow.IsPresent)
			{
				string? stopPattern = !string.IsNullOrWhiteSpace(RegexPattern)
					? RegexPattern
					: (!string.IsNullOrWhiteSpace(promptPattern) || ExpectPrompt.IsPresent)
						? (promptPattern ?? ShellSession.PromptPattern)
						: (!string.IsNullOrWhiteSpace(ExpectText) ? Regex.Escape(ExpectText) : null);
				sendToPipeline = await TransferettoClient.FollowSshShellOutputAsync(ShellSession, timeout, stopPattern, Lookback, options, CancelToken).ConfigureAwait(false);
			}
			else
			{
				sendToPipeline = await TransferettoClient.ReadSshShellAsync(ShellSession, timeout, ReadLine.IsPresent, ExpectText, Lookback, RegexPattern, ReadUntilIdle.IsPresent, idleTimeout, ExpectPrompt.IsPresent, promptPattern, options, CancelToken).ConfigureAwait(false);
			}
			WriteObject(sendToPipeline);
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadSshShellFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}
