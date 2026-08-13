using System;

namespace Transferetto;
/// <summary>
/// Describes the certificate presented during FTP or FTPS connection setup.
/// </summary>

public sealed class TransferettoFtpCertificateInfo {
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
    /// Gets or sets the certificate validity start time.
    /// </summary>

    public DateTime NotBefore { get; set; }
    /// <summary>
    /// Gets or sets the certificate validity end time.
    /// </summary>

    public DateTime NotAfter { get; set; }
    /// <summary>
    /// Gets or sets the policy validation errors reported by the TLS stack.
    /// </summary>

    public string? PolicyErrors { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the certificate was trusted.
    /// </summary>

    public bool CanTrust { get; set; }
    /// <summary>
    /// Gets or sets the trust source.
    /// </summary>

    public TransferettoFtpCertificateTrustSource TrustSource { get; set; }
    /// <summary>
    /// Gets or sets the known-certificate store path used during validation.
    /// </summary>

    public string? KnownCertificatesPath { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the certificate was persisted to the known-certificate store.
    /// </summary>

    public bool WasPersisted { get; set; }
}
