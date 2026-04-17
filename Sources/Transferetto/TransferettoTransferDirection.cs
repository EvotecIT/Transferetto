namespace Transferetto;
/// <summary>
/// Identifies the direction of a transfer operation.
/// </summary>

public enum TransferettoTransferDirection {
    /// <summary>
    /// Uploads data from the local machine to a remote endpoint.
    /// </summary>
    Upload,
    /// <summary>
    /// Downloads data from a remote endpoint to the local machine.
    /// </summary>
    Download,
    /// <summary>
    /// Transfers data between remote endpoints.
    /// </summary>
    Transfer
}
