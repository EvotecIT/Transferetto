namespace Transferetto;
/// <summary>
/// Specifies how SSH host keys should be validated.
/// </summary>

public enum TransferettoSshHostKeyPolicy {
    /// <summary>
    /// The loose value.
    /// </summary>
    Loose,
    /// <summary>
    /// The known Hosts value.
    /// </summary>
    KnownHosts,
    /// <summary>
    /// The trust On First Use value.
    /// </summary>
    TrustOnFirstUse
}
