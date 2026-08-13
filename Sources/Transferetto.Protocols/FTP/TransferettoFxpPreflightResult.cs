namespace Transferetto;
/// <summary>
/// Represents the outcome of an FXP preflight check.
/// </summary>

public sealed class TransferettoFxpPreflightResult {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; init; } = "TestFxpTransfer";
    /// <summary>
    /// Gets or sets a value indicating whether the preflight checks passed.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the preflight checks passed.
    /// </summary>

    public bool IsSuccess { get; init; }
    /// <summary>
    /// Gets or sets the transfer kind.
    /// </summary>

    public TransferettoFxpTransferKind TransferKind { get; init; }
    /// <summary>
    /// Gets or sets the source host.
    /// </summary>

    public string? SourceHost { get; init; }
    /// <summary>
    /// Gets or sets the source port.
    /// </summary>

    public int SourcePort { get; init; }
    /// <summary>
    /// Gets or sets the destination host.
    /// </summary>

    public string? DestinationHost { get; init; }
    /// <summary>
    /// Gets or sets the destination port.
    /// </summary>

    public int DestinationPort { get; init; }
    /// <summary>
    /// Gets or sets the source path.
    /// </summary>

    public string? SourcePath { get; init; }
    /// <summary>
    /// Gets or sets the destination path.
    /// </summary>

    public string? DestinationPath { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the source session is connected.
    /// </summary>

    public bool SourceConnected { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the destination session is connected.
    /// </summary>

    public bool DestinationConnected { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the source path exists.
    /// </summary>

    public bool SourcePathExists { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the destination parent directory exists.
    /// </summary>

    public bool DestinationParentExists { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the source advertises CPSV support.
    /// </summary>

    public bool SourceSupportsCpsv { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the destination advertises CPSV support.
    /// </summary>

    public bool DestinationSupportsCpsv { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the source advertises SSCN support.
    /// </summary>

    public bool SourceSupportsSscn { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the destination advertises SSCN support.
    /// </summary>

    public bool DestinationSupportsSscn { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the source advertises EPSV support.
    /// </summary>

    public bool SourceSupportsEpsv { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the destination advertises EPSV support.
    /// </summary>

    public bool DestinationSupportsEpsv { get; init; }
    /// <summary>
    /// Gets or sets informational and failure messages collected during preflight.
    /// </summary>

    public string[] Messages { get; init; } = System.Array.Empty<string>();
}
