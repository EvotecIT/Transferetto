namespace Transferetto;
/// <summary>
/// Specifies how an FTP or FTPS server certificate was trusted for the active session.
/// </summary>

public enum TransferettoFtpCertificateTrustSource {
    /// <summary>
    /// The certificate was not trusted.
    /// </summary>
    None,
    /// <summary>
    /// The certificate was trusted by the platform certificate chain policy.
    /// </summary>
    PolicyChain,
    /// <summary>
    /// The certificate matched an expected certificate thumbprint.
    /// </summary>
    ExpectedThumbprint,
    /// <summary>
    /// The certificate matched a known-certificate store entry.
    /// </summary>
    KnownCertificates,
    /// <summary>
    /// The certificate was persisted by trust-on-first-use.
    /// </summary>
    TrustOnFirstUse,
    /// <summary>
    /// Certificate validation was bypassed by explicit accept-any configuration.
    /// </summary>
    AcceptAny
}
