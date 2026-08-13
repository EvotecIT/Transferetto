namespace Transferetto;

/// <summary>
/// Represents one planned synchronization decision.
/// </summary>
public sealed class TransferettoSyncPlanItem {
    /// <summary>
    /// Gets or sets the action that should be performed.
    /// </summary>
    public TransferettoSyncAction Action { get; init; }

    /// <summary>
    /// Gets or sets the synchronization direction that produced the plan item.
    /// </summary>
    public TransferettoSyncDirection Direction { get; init; }

    /// <summary>
    /// Gets or sets the relative path affected by this action.
    /// </summary>
    public string RelativePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the local filesystem path affected by this action.
    /// </summary>
    public string? LocalPath { get; init; }

    /// <summary>
    /// Gets or sets the remote FTP or SFTP path affected by this action.
    /// </summary>
    public string? RemotePath { get; init; }

    /// <summary>
    /// Gets or sets the source manifest entry when the item exists in the source.
    /// </summary>
    public TransferettoSyncEntry? Source { get; init; }

    /// <summary>
    /// Gets or sets the destination manifest entry when the item exists in the destination.
    /// </summary>
    public TransferettoSyncEntry? Destination { get; init; }

    /// <summary>
    /// Gets or sets a short reason for the planned action.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the planned action changes local or remote state.
    /// </summary>
    public bool ChangesDestination => Action != TransferettoSyncAction.Skip;
}
