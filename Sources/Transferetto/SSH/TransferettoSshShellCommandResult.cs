namespace Transferetto;
/// <summary>
/// Represents the result of running a command through an interactive SSH shell.
/// </summary>

public sealed class TransferettoSshShellCommandResult {
    /// <summary>
    /// Gets or sets the command.
    /// </summary>
    public string Command { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the command output.
    /// </summary>

    public string Output { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the exit Code.
    /// </summary>

    public int? ExitCode { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets the marker.
    /// </summary>

    public string Marker { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the prompt Pattern.
    /// </summary>

    public string? PromptPattern { get; init; }
}
