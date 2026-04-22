namespace Transferetto;

internal sealed class TransferettoFtpKnownCertificateEntry {
    /// <summary>
    /// Gets or sets the remote host name.
    /// </summary>
    public string Host { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the remote port number.
    /// </summary>

    public int Port { get; set; }
    /// <summary>
    /// Gets or sets the certificate subject.
    /// </summary>

    public string? Subject { get; set; }
    /// <summary>
    /// Gets or sets the certificate issuer.
    /// </summary>

    public string? Issuer { get; set; }
    /// <summary>
    /// Gets or sets the SHA1 certificate thumbprint.
    /// </summary>

    public string? ThumbprintSHA1 { get; set; }
    /// <summary>
    /// Gets or sets the SHA256 certificate thumbprint.
    /// </summary>

    public string? ThumbprintSHA256 { get; set; }
    /// <summary>
    /// Gets or sets the first seen UTC timestamp.
    /// </summary>

    public string? FirstSeenUtc { get; set; }
    /// <summary>
    /// Gets or sets the last seen UTC timestamp.
    /// </summary>

    public string? LastSeenUtc { get; set; }
}
