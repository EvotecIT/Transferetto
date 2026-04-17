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
    /// Gets or sets expected FTPS certificate thumbprints.
    /// </summary>

    public string[]? ExpectedCertificateThumbprints { get; set; }
    /// <summary>
    /// Gets or sets the FTPS certificate validation policy.
    /// </summary>

    public TransferettoFtpCertificatePolicy CertificatePolicy { get; set; } = TransferettoFtpCertificatePolicy.PolicyChain;
    /// <summary>
    /// Gets or sets the known-certificate store path.
    /// </summary>

    public string? KnownCertificatesPath { get; set; }
    /// <summary>
    /// Gets or sets the FTP control connection timeout, in milliseconds.
    /// </summary>

    public int? ConnectTimeout { get; set; }
    /// <summary>
    /// Gets or sets the FTP control socket read timeout, in milliseconds.
    /// </summary>

    public int? ReadTimeout { get; set; }
    /// <summary>
    /// Gets or sets the FTP data connection timeout, in milliseconds.
    /// </summary>

    public int? DataConnectionConnectTimeout { get; set; }
    /// <summary>
    /// Gets or sets the FTP data socket read timeout, in milliseconds.
    /// </summary>

    public int? DataConnectionReadTimeout { get; set; }
    /// <summary>
    /// Gets or sets the number of retry attempts for verified transfers.
    /// </summary>

    public int? RetryAttempts { get; set; }
    /// <summary>
    /// Gets or sets the number of bytes transferred in a single FTP transfer chunk.
    /// </summary>

    public int? TransferChunkSize { get; set; }
    /// <summary>
    /// Gets or sets the local file buffer size used by FTP transfers.
    /// </summary>

    public int? LocalFileBufferSize { get; set; }
    /// <summary>
    /// Gets or sets the internet protocol versions allowed for FTP connections.
    /// </summary>

    public FtpIpVersion? InternetProtocolVersions { get; set; }
    /// <summary>
    /// Gets or sets the upload rate limit in kilobytes per second.
    /// </summary>

    public uint? UploadRateLimit { get; set; }
    /// <summary>
    /// Gets or sets the download rate limit in kilobytes per second.
    /// </summary>

    public uint? DownloadRateLimit { get; set; }
    /// <summary>
    /// Gets or sets the data type used by high-level FTP uploads.
    /// </summary>

    public FtpDataType? UploadDataType { get; set; }
    /// <summary>
    /// Gets or sets the data type used by high-level FTP downloads.
    /// </summary>

    public FtpDataType? DownloadDataType { get; set; }
    /// <summary>
    /// Gets or sets the data type used by FTP directory listings.
    /// </summary>

    public FtpDataType? ListingDataType { get; set; }
    /// <summary>
    /// Gets or sets the data type used by FXP server-to-server transfers.
    /// </summary>

    public FtpDataType? FXPDataType { get; set; }
    /// <summary>
    /// Gets or sets how often FXP progress is reported.
    /// </summary>

    public int? FXPProgressInterval { get; set; }
    /// <summary>
    /// Gets or sets active-mode data ports.
    /// </summary>

    public int[]? ActivePorts { get; set; }
    /// <summary>
    /// Gets or sets passive-mode ports to avoid.
    /// </summary>

    public int[]? PassiveBlockedPorts { get; set; }
    /// <summary>
    /// Gets or sets the maximum number of passive-mode connection attempts.
    /// </summary>

    public int? PassiveMaxAttempts { get; set; }
    /// <summary>
    /// Gets or sets the text encoding name used by the FTP control channel.
    /// </summary>

    public string? EncodingName { get; set; }
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
