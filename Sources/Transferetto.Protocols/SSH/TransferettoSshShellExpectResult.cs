using System;
using System.Collections.Generic;

namespace Transferetto;
/// <summary>
/// Represents the result of an ordered interactive SSH shell expect workflow.
/// </summary>

public sealed class TransferettoSshShellExpectResult {
    /// <summary>
    /// Gets or sets a value indicating whether every executed step succeeded.
    /// </summary>
    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets the index of the first failed step when the workflow does not succeed.
    /// </summary>

    public int? FailedStepIndex { get; init; }
    /// <summary>
    /// Gets or sets the number of steps that were executed.
    /// </summary>

    public int CompletedStepCount { get; init; }
    /// <summary>
    /// Gets or sets the ordered step results.
    /// </summary>

    public IReadOnlyList<TransferettoSshShellExpectStepResult> Steps { get; init; } = Array.Empty<TransferettoSshShellExpectStepResult>();
}
