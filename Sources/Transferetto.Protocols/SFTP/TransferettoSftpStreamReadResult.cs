namespace Transferetto;
/// <summary>
/// Represents the result of reading from an SFTP stream.
/// </summary>

public sealed class TransferettoSftpStreamReadResult {
    /// <summary>
    /// Gets or sets the action name reported for the operation.
    /// </summary>
    public string Action { get; init; } = "ReadStream";
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>

    public bool Status { get; init; }
    /// <summary>
    /// Gets or sets the path associated with the operation.
    /// </summary>

    public string Path { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the data buffer returned by the stream read operation.
    /// </summary>

    public byte[] Data { get; init; } = [];
    /// <summary>
    /// Gets or sets the bytes Read.
    /// </summary>

    public int BytesRead { get; init; }
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public long Position { get; init; }
    /// <summary>
    /// Gets or sets the end Of Stream.
    /// </summary>

    public bool EndOfStream { get; init; }
}
