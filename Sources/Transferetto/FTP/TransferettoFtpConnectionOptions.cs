using System.Net;
using FluentFTP;

namespace Transferetto;
/// <summary>
/// Represents connection settings for an FTP or FTPS session.
/// </summary>

public sealed class TransferettoFtpConnectionOptions {
    /// <summary>
    /// Gets or sets the server.
    /// </summary>
    public string? Server { get; set; }
    /// <summary>
    /// Gets or sets the remote port number.
    /// </summary>

    public int? Port { get; set; }
    /// <summary>
    /// Gets or sets the credential.
    /// </summary>

    public NetworkCredential? Credential { get; set; }
    /// <summary>
    /// Gets or sets the ftp Profile.
    /// </summary>

    public FtpProfile? FtpProfile { get; set; }
    /// <summary>
    /// Gets or sets the encryption Mode.
    /// </summary>

    public FtpEncryptionMode[]? EncryptionMode { get; set; }
    /// <summary>
    /// Gets or sets the data Connection Type.
    /// </summary>

    public FtpDataConnectionType? DataConnectionType { get; set; }
    /// <summary>
    /// Gets or sets the ssl Buffering.
    /// </summary>

    public FtpsBuffering? SslBuffering { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether disable Data Connection Encryption.
    /// </summary>

    public bool DisableDataConnectionEncryption { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether disable Validate Certificate Revocation.
    /// </summary>

    public bool DisableValidateCertificateRevocation { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether validate Any Certificate.
    /// </summary>

    public bool ValidateAnyCertificate { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether send Host.
    /// </summary>

    public bool SendHost { get; set; }
    /// <summary>
    /// Gets or sets the socket Keep Alive.
    /// </summary>

    public bool SocketKeepAlive { get; set; }
    /// <summary>
    /// Gets or sets the auto Connect.
    /// </summary>

    public bool AutoConnect { get; set; }
    /// <summary>
    /// Gets or sets the proxy Options.
    /// </summary>

    public TransferettoFtpProxyOptions? ProxyOptions { get; set; }
    /// <summary>
    /// Gets or sets the trace Options.
    /// </summary>

    public TransferettoFtpTraceOptions? TraceOptions { get; set; }
}
