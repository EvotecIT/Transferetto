using System;

namespace Transferetto;
/// <summary>
/// Represents the result of a reusable SSH shell recipe execution.
/// </summary>

public sealed class TransferettoSshShellRecipeResult {
    /// <summary>
    /// Gets or sets the executed recipe kind.
    /// </summary>
    public TransferettoSshShellRecipeKind Recipe { get; init; }
    /// <summary>
    /// Gets or sets the shell command text sent for the recipe.
    /// </summary>

    public string CommandText { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the captured shell output.
    /// </summary>

    public string Output { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the exit code when the recipe captures one.
    /// </summary>

    public int? ExitCode { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the recipe completed successfully.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the recipe stopped a long-running remote command with an interrupt.
    /// </summary>

    public bool WasInterrupted { get; init; }
    /// <summary>
    /// Gets or sets the prompt pattern used while returning to the interactive shell.
    /// </summary>

    public string? PromptPattern { get; init; }
    /// <summary>
    /// Gets or sets the UTC timestamp when the recipe started.
    /// </summary>

    public DateTime StartedUtc { get; init; }
    /// <summary>
    /// Gets or sets the UTC timestamp when the recipe completed.
    /// </summary>

    public DateTime CompletedUtc { get; init; }
}
