namespace Transferetto;
/// <summary>
/// Represents the result of writing to an FTP stream.
/// </summary>

public sealed class TransferettoFtpStreamWriteResult {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; init; } = "WriteStream";
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets the path associated with the operation.
    /// </summary>

    public string Path { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the bytes Written.
    /// </summary>

    public int BytesWritten { get; init; }
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public long Position { get; init; }
    /// <summary>
    /// Gets or sets the message reported for the operation.
    /// </summary>

    public string Message { get; init; } = string.Empty;
}
