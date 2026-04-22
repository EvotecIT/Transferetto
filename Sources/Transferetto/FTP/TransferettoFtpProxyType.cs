namespace Transferetto;
/// <summary>
/// Specifies the proxy protocol used for FTP or FTPS traffic.
/// </summary>

public enum TransferettoFtpProxyType {
    /// <summary>
    /// The ftp Client Socks5 Proxy value.
    /// </summary>
    FtpClientSocks5Proxy,
    /// <summary>
    /// The ftp Client Http11 Proxy value.
    /// </summary>
    FtpClientHttp11Proxy,
    /// <summary>
    /// The ftp Client Socks4a Proxy value.
    /// </summary>
    FtpClientSocks4aProxy,
    /// <summary>
    /// The ftp Client Socks4 Proxy value.
    /// </summary>
    FtpClientSocks4Proxy,
    /// <summary>
    /// The ftp Client User At Host Proxy value.
    /// </summary>
    FtpClientUserAtHostProxy,
    /// <summary>
    /// The ftp Client Blue Coat Proxy value.
    /// </summary>
    FtpClientBlueCoatProxy
}
