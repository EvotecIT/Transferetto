namespace Transferetto;
/// <summary>
/// Specifies the proxy protocol used for SSH-based sessions.
/// </summary>

public enum TransferettoSshProxyType {
    /// <summary>
    /// The none value.
    /// </summary>
    None,
    /// <summary>
    /// The http value.
    /// </summary>
    Http,
    /// <summary>
    /// The socks4 value.
    /// </summary>
    Socks4,
    /// <summary>
    /// The socks5 value.
    /// </summary>
    Socks5
}
