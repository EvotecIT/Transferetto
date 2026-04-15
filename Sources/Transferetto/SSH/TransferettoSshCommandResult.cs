namespace Transferetto;
/// <summary>
/// Represents the result of a non-interactive SSH command execution.
/// </summary>

public sealed class TransferettoSshCommandResult {
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Status { get; set; }
    /// <summary>
    /// Gets or sets the command output.
    /// </summary>

    public string Output { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the error message associated with the current session or operation.
    /// </summary>

    public string? Error { get; set; }
}
