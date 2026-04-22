using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Runs one or more non-interactive SSH commands and captures their output.</para>
/// <para type="description">Supports multi-line command blocks, structured status results, progressive stdout and stderr streaming, and per-command timeouts on top of the reusable SSH command execution layer.</para>
/// <example>
///   <para>Run two simple commands and return the combined stdout text.</para>
///   <code>Send-SSHCommand -SshClient $ssh -Command { 'pwd'; 'uname -a' }</code>
/// </example>
/// <example>
///   <para>Return the structured command result with exit status and timestamps.</para>
///   <code>Send-SSHCommand -SshClient $ssh -Command { 'tail -n 20 /var/log/syslog' } -Status</code>
/// </example>
/// <example>
///   <para>Stream output from a longer-running command and apply a timeout.</para>
///   <code>Send-SSHCommand -SshClient $ssh -Command { 'long-running-script.sh' } -StreamOutput -CommandTimeoutSeconds 30 -Status</code>
/// </example>
/// </summary>
[Cmdlet("Send", "SSHCommand")]
public sealed class CmdletSendSshCommand : AsyncPSCmdlet
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

	/// <summary>
	/// Gets or sets a value indicating whether progressive command output chunks are written to the pipeline.
	/// </summary>
	[Parameter]
	public SwitchParameter StreamOutput { get; set; }

	/// <summary>
	/// Gets or sets the timeout, in seconds, applied to the remote command.
	/// </summary>
	[Parameter]
	public int? CommandTimeoutSeconds { get; set; }

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync()
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
			TransferettoSshCommandOptions options = new() {
				CancellationToken = CancelToken,
				OutputProgress = StreamOutput.IsPresent ? new TransferettoSshCommandOutputProgress(this) : null
			};
			if (CommandTimeoutSeconds.HasValue) {
				if (CommandTimeoutSeconds.Value <= 0) {
					throw new PSArgumentOutOfRangeException(nameof(CommandTimeoutSeconds), CommandTimeoutSeconds.Value, "CommandTimeoutSeconds must be greater than zero.");
				}

				options.CommandTimeout = TimeSpan.FromSeconds(CommandTimeoutSeconds.Value);
			}

			TransferettoSshCommandResult transferettoSshCommandResult = await TransferettoClient.SendSshCommandAsync(SshClient, commands, options, CancelToken).ConfigureAwait(false);
			if (Status.IsPresent)
			{
				WriteObject(transferettoSshCommandResult);
			}
			else if (!StreamOutput.IsPresent)
			{
				WriteObject(transferettoSshCommandResult.Output);
			}
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSshCommandFailed", ErrorCategory.WriteError, SshClient.Host));
		}
	}
}
