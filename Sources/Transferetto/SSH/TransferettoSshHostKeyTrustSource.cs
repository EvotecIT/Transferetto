namespace Transferetto;
/// <summary>
/// Specifies how an SSH host key was trusted for the active session.
/// </summary>

public enum TransferettoSshHostKeyTrustSource {
    /// <summary>
    /// The none value.
    /// </summary>
    None,
    /// <summary>
    /// The loose value.
    /// </summary>
    Loose,
    /// <summary>
    /// The accept Any value.
    /// </summary>
    AcceptAny,
    /// <summary>
    /// The expected Fingerprint value.
    /// </summary>
    ExpectedFingerprint,
    /// <summary>
    /// The known Hosts value.
    /// </summary>
    KnownHosts,
    /// <summary>
    /// The trust On First Use value.
    /// </summary>
    TrustOnFirstUse
}
