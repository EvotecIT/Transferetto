using System.Net;

namespace Transferetto;
/// <summary>
/// Represents proxy settings for an FTP or FTPS session.
/// </summary>

public sealed class TransferettoFtpProxyOptions {
    /// <summary>
    /// Gets or sets the proxy Host.
    /// </summary>
    public string ProxyHost { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the proxy Port.
    /// </summary>

    public int ProxyPort { get; set; }
    /// <summary>
    /// Gets or sets the proxy Credential.
    /// </summary>

    public NetworkCredential? ProxyCredential { get; set; }
    /// <summary>
    /// Gets or sets the proxy Type.
    /// </summary>

    public TransferettoFtpProxyType ProxyType { get; set; }
}
