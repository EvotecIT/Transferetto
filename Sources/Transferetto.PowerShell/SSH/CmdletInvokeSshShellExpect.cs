using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Executes an ordered expect-style workflow against an interactive SSH shell.</para>
/// <para type="description">Supports send-text, control-key, prompt, text, regex, line, idle, and follow-mode steps through reusable step objects or PSCustomObject input, making it possible to script interactive shell flows without dropping into raw shell polling logic.</para>
/// <example>
///   <para>Run two simple shell commands and wait for a Linux prompt after each one.</para>
///   <code>$steps = @(
///   [pscustomobject]@{ Name = 'pwd'; SendText = 'pwd'; ExpectPrompt = $true; PromptPreset = 'Linux' },
///   [pscustomobject]@{ Name = 'list'; SendText = 'ls -la /srv/app'; ExpectPrompt = $true; PromptPreset = 'Linux' }
/// )
/// Invoke-SSHShellExpect -ShellSession $shell -Step $steps</code>
/// </example>
/// <example>
///   <para>Follow shell output from a long-running command until a stop pattern is observed.</para>
///   <code>$steps = @(
///   [pscustomobject]@{ SendText = 'tail -n 20 -f /var/log/app.log'; Follow = $true; StopPattern = 'ready'; TimeoutSeconds = 30 }
/// )
/// Invoke-SSHShellExpect -ShellSession $shell -Step $steps -StreamOutput</code>
/// </example>
/// </summary>
[Cmdlet("Invoke", "SSHShellExpect")]
public sealed class CmdletInvokeSshShellExpect : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the shell session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the ordered expect steps.
	/// </summary>

	[Alias(new string[] { "Steps" })]
	[Parameter(Mandatory = true)]
	public PSObject[]? Step { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether progressive shell output chunks are written to the pipeline while steps execute.
	/// </summary>

	[Parameter]
	public SwitchParameter StreamOutput { get; set; }
	/// <summary>
	/// Gets or sets the poll interval, in milliseconds, used while waiting for shell output.
	/// </summary>

	[Parameter]
	public int PollIntervalMilliseconds { get; set; } = 50;

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync() {
		if (ShellSession == null || Step == null || Step.Length == 0) {
			return;
		}

		try {
			TransferettoSshShellReadOptions options = new() {
				CancellationToken = CancelToken,
				OutputProgress = StreamOutput.IsPresent ? new TransferettoSshShellOutputProgress(this) : null,
				PollInterval = TimeSpan.FromMilliseconds(PollIntervalMilliseconds > 0 ? PollIntervalMilliseconds : 50)
			};
			TransferettoSshShellExpectStep[] steps = ConvertSteps(Step);
			TransferettoSshShellExpectResult result = await TransferettoClient.InvokeSshShellExpectAsync(ShellSession, steps, options, CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "InvokeSshShellExpectFailed", ErrorCategory.OperationTimeout, ShellSession.Host));
		}
	}

	private static TransferettoSshShellExpectStep[] ConvertSteps(IEnumerable<PSObject> steps) {
		List<TransferettoSshShellExpectStep> results = new();
		foreach (PSObject step in steps) {
			if (step.BaseObject is TransferettoSshShellExpectStep typedStep) {
				results.Add(typedStep);
				continue;
			}

			results.Add(new TransferettoSshShellExpectStep {
				Name = GetValue<string>(step, "Name"),
				SendText = GetValue<string>(step, "SendText") ?? GetValue<string>(step, "Text"),
				AppendLine = !(GetValue<bool?>(step, "NoNewLine") ?? false) && (GetValue<bool?>(step, "AppendLine") ?? true),
				ControlKey = GetValue<TransferettoSshShellControlKey?>(step, "ControlKey") ?? GetValue<TransferettoSshShellControlKey?>(step, "Key"),
				ControlRepeat = GetValue<int?>(step, "ControlRepeat") ?? GetValue<int?>(step, "Repeat") ?? 1,
				ExpectText = GetValue<string>(step, "ExpectText"),
				RegexPattern = GetValue<string>(step, "RegexPattern"),
				ExpectPrompt = GetValue<bool?>(step, "ExpectPrompt") ?? false,
				PromptPattern = GetValue<string>(step, "PromptPattern"),
				PromptPreset = GetValue<TransferettoSshShellPromptPreset?>(step, "PromptPreset") ?? TransferettoSshShellPromptPreset.None,
				ReadLine = GetValue<bool?>(step, "ReadLine") ?? false,
				ReadUntilIdle = GetValue<bool?>(step, "ReadUntilIdle") ?? false,
				Follow = GetValue<bool?>(step, "Follow") ?? false,
				StopPattern = GetValue<string>(step, "StopPattern"),
				Lookback = GetValue<int?>(step, "Lookback") ?? -1,
				Timeout = GetSeconds(step, "TimeoutSeconds") ?? GetValue<TimeSpan?>(step, "Timeout"),
				IdleTimeout = GetSeconds(step, "IdleTimeoutSeconds") ?? GetValue<TimeSpan?>(step, "IdleTimeout")
			});
		}

		return results.ToArray();
	}

	private static TimeSpan? GetSeconds(PSObject step, string propertyName) {
		double? seconds = GetValue<double?>(step, propertyName);
		return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : null;
	}

	private static T? GetValue<T>(PSObject step, string propertyName) {
		PSPropertyInfo? property = step.Properties[propertyName];
		if (property?.Value == null) {
			return default;
		}

		if (LanguagePrimitives.TryConvertTo(property.Value, out T value)) {
			return value;
		}

		return default;
	}
}
