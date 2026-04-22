using System;

namespace Transferetto;
/// <summary>
/// Represents the result of a non-interactive SSH command execution.
/// </summary>

public sealed class TransferettoSshCommandResult {
    /// <summary>
    /// Gets or sets the command text that was executed remotely.
    /// </summary>
    public string CommandText { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Status { get; set; }
    /// <summary>
    /// Gets or sets the remote exit status.
    /// </summary>

    public int? ExitStatus { get; set; }
    /// <summary>
    /// Gets or sets the remote exit signal, when available.
    /// </summary>

    public string? ExitSignal { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the command was canceled.
    /// </summary>

    public bool IsCanceled { get; set; }
    /// <summary>
    /// Gets or sets the command output.
    /// </summary>

    public string Output { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the error message associated with the current session or operation.
    /// </summary>

    public string? Error { get; set; }
    /// <summary>
    /// Gets or sets the UTC timestamp when command execution started.
    /// </summary>

    public DateTime StartedUtc { get; set; }
    /// <summary>
    /// Gets or sets the UTC timestamp when command execution completed.
    /// </summary>

    public DateTime CompletedUtc { get; set; }
}
