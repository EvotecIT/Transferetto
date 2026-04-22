using System;

namespace Transferetto;
/// <summary>
/// Represents one ordered step in an interactive SSH shell expect workflow.
/// </summary>

public sealed class TransferettoSshShellExpectStep {
    /// <summary>
    /// Gets or sets the optional step name.
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// Gets or sets the text to send before reading output.
    /// </summary>

    public string? SendText { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether a newline is appended when sending text.
    /// </summary>

    public bool AppendLine { get; init; } = true;
    /// <summary>
    /// Gets or sets the optional control key to send before reading output.
    /// </summary>

    public TransferettoSshShellControlKey? ControlKey { get; init; }
    /// <summary>
    /// Gets or sets the number of times the control key should be sent.
    /// </summary>

    public int ControlRepeat { get; init; } = 1;
    /// <summary>
    /// Gets or sets the text to wait for in the shell output.
    /// </summary>

    public string? ExpectText { get; init; }
    /// <summary>
    /// Gets or sets the regular expression to wait for in the shell output.
    /// </summary>

    public string? RegexPattern { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the prompt should be waited for.
    /// </summary>

    public bool ExpectPrompt { get; init; }
    /// <summary>
    /// Gets or sets the prompt pattern to use when waiting for a prompt.
    /// </summary>

    public string? PromptPattern { get; init; }
    /// <summary>
    /// Gets or sets the reusable prompt preset to use when waiting for a prompt.
    /// </summary>

    public TransferettoSshShellPromptPreset PromptPreset { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether one line should be read.
    /// </summary>

    public bool ReadLine { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether output should be read until idle.
    /// </summary>

    public bool ReadUntilIdle { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether shell output should be followed until timeout, cancellation, or stop condition.
    /// </summary>

    public bool Follow { get; init; }
    /// <summary>
    /// Gets or sets the optional stop pattern used during follow mode.
    /// </summary>

    public string? StopPattern { get; init; }
    /// <summary>
    /// Gets or sets the lookback window used for text and regex matching.
    /// </summary>

    public int Lookback { get; init; } = -1;
    /// <summary>
    /// Gets or sets the timeout applied to the read or wait portion of the step.
    /// </summary>

    public TimeSpan? Timeout { get; init; }
    /// <summary>
    /// Gets or sets the idle timeout used when reading until idle.
    /// </summary>

    public TimeSpan? IdleTimeout { get; init; }
}
