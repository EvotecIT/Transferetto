namespace Transferetto;
/// <summary>
/// Specifies how FTP or FTPS server certificates should be validated.
/// </summary>

public enum TransferettoFtpCertificatePolicy {
    /// <summary>
    /// Trust certificates accepted by the platform certificate chain policy.
    /// </summary>
    PolicyChain,
    /// <summary>
    /// Trust only certificates already present in the known-certificate store.
    /// </summary>
    KnownCertificates,
    /// <summary>
    /// Persist the first certificate seen for a host and trust matching certificates on future connections.
    /// </summary>
    TrustOnFirstUse
}
