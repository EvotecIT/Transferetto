using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SSHCommand cmdlet.
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
