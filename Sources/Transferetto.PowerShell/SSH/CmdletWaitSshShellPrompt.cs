using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Wait-SSHShellPrompt cmdlet.
/// </summary>

[Cmdlet("Wait", "SSHShellPrompt")]
public sealed class CmdletWaitSshShellPrompt : AsyncPSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
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
	/// <summary>
	/// Gets or sets a value indicating whether progressive output chunks are written to the pipeline while waiting for the prompt.
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
			string? promptPattern = TransferettoClient.ResolveSshShellPromptPattern(PromptPattern, PromptPreset);
			TransferettoSshShellReadOptions options = new() {
				CancellationToken = CancelToken,
				OutputProgress = StreamOutput.IsPresent ? new TransferettoSshShellOutputProgress(this) : null,
				PollInterval = TimeSpan.FromMilliseconds(PollIntervalMilliseconds > 0 ? PollIntervalMilliseconds : 50)
			};
			WriteObject(await TransferettoClient.WaitForSshShellPromptAsync(ShellSession, timeout, promptPattern, Lookback, options, CancelToken).ConfigureAwait(false));
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "WaitSshShellPromptFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}
