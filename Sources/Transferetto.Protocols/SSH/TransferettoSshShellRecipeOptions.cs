using System;

namespace Transferetto;
/// <summary>
/// Represents reusable SSH shell recipe options for common Linux administration flows.
/// </summary>

public sealed class TransferettoSshShellRecipeOptions {
    /// <summary>
    /// Gets or sets the recipe kind to execute.
    /// </summary>
    public TransferettoSshShellRecipeKind Recipe { get; set; }
    /// <summary>
    /// Gets or sets the command used by recipes that execute a shell command.
    /// </summary>

    public string? Command { get; set; }
    /// <summary>
    /// Gets or sets the sudo password used by <see cref="TransferettoSshShellRecipeKind.SudoCommand"/>.
    /// </summary>

    public string? Password { get; set; }
    /// <summary>
    /// Gets or sets the regular expression used to detect a sudo password prompt when it must be overridden.
    /// </summary>

    public string? PasswordPromptPattern { get; set; }
    /// <summary>
    /// Gets or sets the remote file path used by <see cref="TransferettoSshShellRecipeKind.FollowFile"/>.
    /// </summary>

    public string? RemotePath { get; set; }
    /// <summary>
    /// Gets or sets the service name used by <see cref="TransferettoSshShellRecipeKind.FollowJournal"/>.
    /// </summary>

    public string? ServiceName { get; set; }
    /// <summary>
    /// Gets or sets the number of historical lines to include before entering follow mode.
    /// </summary>

    public int TailLines { get; set; } = 200;
    /// <summary>
    /// Gets or sets the optional stop pattern used while following output.
    /// </summary>

    public string? StopPattern { get; set; }
    /// <summary>
    /// Gets or sets the lookback window used for text and regular expression matching.
    /// </summary>

    public int Lookback { get; set; } = -1;
    /// <summary>
    /// Gets or sets the recipe timeout.
    /// </summary>

    public TimeSpan? Timeout { get; set; }
    /// <summary>
    /// Gets or sets the timeout used when interrupting a long-running follow command.
    /// </summary>

    public TimeSpan? InterruptTimeout { get; set; } = TimeSpan.FromSeconds(5);
    /// <summary>
    /// Gets or sets the explicit prompt pattern used when a recipe needs to return to the shell prompt.
    /// </summary>

    public string? PromptPattern { get; set; }
    /// <summary>
    /// Gets or sets the reusable prompt preset used when <see cref="PromptPattern"/> is not provided.
    /// </summary>

    public TransferettoSshShellPromptPreset PromptPreset { get; set; }
}
