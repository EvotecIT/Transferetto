namespace Transferetto;
/// <summary>
/// Represents the outcome of a non-stream Transferetto operation.
/// </summary>

public sealed class TransferettoOperationResult {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>

    public bool Status { get; set; }
    /// <summary>
    /// Gets or sets the message reported for the operation.
    /// </summary>

    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the path associated with the operation.
    /// </summary>

    public string? Path { get; set; }
    /// <summary>
    /// Gets or sets the original path associated with the operation.
    /// </summary>

    public string? OldPath { get; set; }
    /// <summary>
    /// Gets or sets the new path associated with the operation.
    /// </summary>

    public string? NewPath { get; set; }
}
