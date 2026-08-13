using System;

namespace Transferetto;
/// <summary>
/// Represents the result of a single step in an interactive SSH shell expect workflow.
/// </summary>

public sealed class TransferettoSshShellExpectStepResult {
    /// <summary>
    /// Gets or sets the zero-based step index.
    /// </summary>
    public int StepIndex { get; init; }
    /// <summary>
    /// Gets or sets the optional step name.
    /// </summary>

    public string? StepName { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the step succeeded.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the step matched its expected condition.
    /// </summary>

    public bool Matched { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the step timed out before matching.
    /// </summary>

    public bool TimedOut { get; init; }
    /// <summary>
    /// Gets or sets the output captured for the step.
    /// </summary>

    public string Output { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the UTC timestamp when the step started.
    /// </summary>

    public DateTime StartedUtc { get; init; }
    /// <summary>
    /// Gets or sets the UTC timestamp when the step completed.
    /// </summary>

    public DateTime CompletedUtc { get; init; }
}
