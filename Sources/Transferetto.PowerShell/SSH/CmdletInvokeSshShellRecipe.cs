using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Runs a reusable Linux administration recipe inside an interactive SSH shell.</para>
/// <para type="description">Provides a higher-level shell automation layer for common administration flows such as running a sudo command, following a remote file with tail, or following systemd logs with journalctl, while still supporting prompt presets, streaming output, and cancellation.</para>
/// <example>
///   <para>Run a privileged command through sudo inside the active shell session.</para>
///   <code>Invoke-SSHShellRecipe -ShellSession $shell -Recipe SudoCommand -Command 'systemctl restart nginx' -Password 'secret' -PromptPreset LinuxRoot</code>
/// </example>
/// <example>
///   <para>Follow a remote log file for 30 seconds and stream the output as it arrives.</para>
///   <code>Invoke-SSHShellRecipe -ShellSession $shell -Recipe FollowFile -RemotePath '/var/log/nginx/access.log' -TailLines 50 -TimeoutSeconds 30 -StreamOutput</code>
/// </example>
/// <example>
///   <para>Follow systemd journal output for one service.</para>
///   <code>Invoke-SSHShellRecipe -ShellSession $shell -Recipe FollowJournal -ServiceName 'nginx' -TailLines 100 -TimeoutSeconds 60 -StreamOutput</code>
/// </example>
/// </summary>
[Cmdlet("Invoke", "SSHShellRecipe")]
public sealed class CmdletInvokeSshShellRecipe : AsyncPSCmdlet {
    /// <summary>
    /// Gets or sets the shell session.
    /// </summary>
    [Parameter(Mandatory = true)]
    public TransferettoSshShellSession? ShellSession { get; set; }
    /// <summary>
    /// Gets or sets the reusable recipe kind.
    /// </summary>

    [Parameter(Mandatory = true)]
    public TransferettoSshShellRecipeKind Recipe { get; set; }
    /// <summary>
    /// Gets or sets the command used by the sudo recipe.
    /// </summary>

    [Parameter]
    public string? Command { get; set; }
    /// <summary>
    /// Gets or sets the sudo password used by the sudo recipe.
    /// </summary>

    [Parameter]
    public string? Password { get; set; }
    /// <summary>
    /// Gets or sets the sudo password prompt pattern when it must be overridden.
    /// </summary>

    [Parameter]
    public string? PasswordPromptPattern { get; set; }
    /// <summary>
    /// Gets or sets the remote path used by the file-follow recipe.
    /// </summary>

    [Parameter]
    public string? RemotePath { get; set; }
    /// <summary>
    /// Gets or sets the service name used by the journal-follow recipe.
    /// </summary>

    [Parameter]
    public string? ServiceName { get; set; }
    /// <summary>
    /// Gets or sets the number of lines shown before follow mode starts.
    /// </summary>

    [Parameter]
    public int TailLines { get; set; } = 200;
    /// <summary>
    /// Gets or sets the optional stop pattern used while following output.
    /// </summary>

    [Parameter]
    public string? StopPattern { get; set; }
    /// <summary>
    /// Gets or sets the lookback window used for text and regular expression matching.
    /// </summary>

    [Parameter]
    public int Lookback { get; set; } = -1;
    /// <summary>
    /// Gets or sets the recipe timeout, in seconds.
    /// </summary>

    [Parameter]
    public double TimeoutSeconds { get; set; } = -1.0;
    /// <summary>
    /// Gets or sets the timeout used to interrupt a long-running follow recipe, in seconds.
    /// </summary>

    [Parameter]
    public double InterruptTimeoutSeconds { get; set; } = 5.0;
    /// <summary>
    /// Gets or sets the explicit prompt pattern used when returning to the shell prompt.
    /// </summary>

    [Parameter]
    public string? PromptPattern { get; set; }
    /// <summary>
    /// Gets or sets the reusable prompt preset used when <see cref="PromptPattern"/> is not supplied.
    /// </summary>

    [Parameter]
    public TransferettoSshShellPromptPreset PromptPreset { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether progressive shell output chunks are written to the pipeline while the recipe runs.
    /// </summary>

    [Parameter]
    public SwitchParameter StreamOutput { get; set; }
    /// <summary>
    /// Gets or sets the poll interval, in milliseconds, used while waiting for shell output.
    /// </summary>

    [Parameter]
    public int PollIntervalMilliseconds { get; set; } = 50;
    /// <summary>
    /// Gets or sets a value indicating whether only the raw output text should be written to the pipeline.
    /// </summary>

    [Parameter]
    public SwitchParameter RawOutput { get; set; }

    /// <inheritdoc/>
    protected override async Task ProcessRecordAsync() {
        if (ShellSession == null) {
            return;
        }

        try {
            TransferettoSshShellRecipeOptions recipeOptions = new() {
                Recipe = Recipe,
                Command = Command,
                Password = Password,
                PasswordPromptPattern = PasswordPromptPattern,
                RemotePath = RemotePath,
                ServiceName = ServiceName,
                TailLines = TailLines,
                StopPattern = StopPattern,
                Lookback = Lookback,
                Timeout = MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds") ? TimeSpan.FromSeconds(TimeoutSeconds) : null,
                InterruptTimeout = MyInvocation.BoundParameters.ContainsKey("InterruptTimeoutSeconds") ? TimeSpan.FromSeconds(InterruptTimeoutSeconds) : null,
                PromptPattern = PromptPattern,
                PromptPreset = PromptPreset
            };
            TransferettoSshShellReadOptions readOptions = new() {
                CancellationToken = CancelToken,
                OutputProgress = StreamOutput.IsPresent ? new TransferettoSshShellOutputProgress(this) : null,
                PollInterval = TimeSpan.FromMilliseconds(PollIntervalMilliseconds > 0 ? PollIntervalMilliseconds : 50)
            };

            TransferettoSshShellRecipeResult result = await TransferettoClient.InvokeSshShellRecipeAsync(ShellSession, recipeOptions, readOptions, CancelToken).ConfigureAwait(false);
            WriteObject(RawOutput.IsPresent ? result.Output : result);
        } catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
            // StopProcessing requested cancellation.
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "InvokeSshShellRecipeFailed", ErrorCategory.OperationTimeout, ShellSession.Host));
        }
    }
}
