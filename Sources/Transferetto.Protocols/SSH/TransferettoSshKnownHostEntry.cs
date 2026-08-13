namespace Transferetto;

internal sealed class TransferettoSshKnownHostEntry {
    /// <summary>
    /// Gets or sets the remote host name.
    /// </summary>
    public string Host { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the remote port number.
    /// </summary>

    public int Port { get; set; }
    /// <summary>
    /// Gets or sets the host Key Name.
    /// </summary>

    public string HostKeyName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the finger Print MD5.
    /// </summary>

    public string? FingerPrintMD5 { get; set; }
    /// <summary>
    /// Gets or sets the finger Print SHA256.
    /// </summary>

    public string? FingerPrintSHA256 { get; set; }
    /// <summary>
    /// Gets or sets the key Length.
    /// </summary>

    public int KeyLength { get; set; }
    /// <summary>
    /// Gets or sets the first Seen Utc.
    /// </summary>

    public string? FirstSeenUtc { get; set; }
    /// <summary>
    /// Gets or sets the last Seen Utc.
    /// </summary>

    public string? LastSeenUtc { get; set; }
}
