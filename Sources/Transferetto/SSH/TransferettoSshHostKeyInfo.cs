namespace Transferetto;
/// <summary>
/// Describes the host key that was presented during SSH connection setup.
/// </summary>

public sealed class TransferettoSshHostKeyInfo {
    /// <summary>
    /// Gets or sets the host Key Name.
    /// </summary>
    public string HostKeyName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the key Length.
    /// </summary>

    public int KeyLength { get; set; }
    /// <summary>
    /// Gets or sets the finger Print MD5.
    /// </summary>

    public string? FingerPrintMD5 { get; set; }
    /// <summary>
    /// Gets or sets the finger Print SHA256.
    /// </summary>

    public string? FingerPrintSHA256 { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether trust.
    /// </summary>

    public bool CanTrust { get; set; }
    /// <summary>
    /// Gets or sets the trust Source.
    /// </summary>

    public TransferettoSshHostKeyTrustSource TrustSource { get; set; }
    /// <summary>
    /// Gets or sets the known Hosts Path.
    /// </summary>

    public string? KnownHostsPath { get; set; }
    /// <summary>
    /// Gets or sets the was Persisted.
    /// </summary>

    public bool WasPersisted { get; set; }
}
