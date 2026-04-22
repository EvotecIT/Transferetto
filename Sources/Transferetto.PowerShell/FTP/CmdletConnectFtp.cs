using System;
using System.Management.Automation;
using System.Net;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates an FTP or FTPS session with runtime tuning, proxy support, and certificate trust controls.</para>
/// <para type="description">Supports classic username/password and credential-based authentication, FluentFTP profiles, FTPS encryption modes, proxy settings, trust-on-first-use and known-certificate validation, plus transfer/runtime tuning that can be reused by later FTP and FTPS cmdlets.</para>
/// <example>
///   <para>Connect to a plain FTP server with a credential prompt.</para>
///   <code>$ftp = Connect-FTP -Server 'test.rebex.net' -Credential (Get-Credential)</code>
/// </example>
/// <example>
///   <para>Connect to an FTPS endpoint and pin trust in a reusable known-certificate store.</para>
///   <code>$ftp = Connect-FTP -Server 'ftps.example.com' -Credential (Get-Credential) -EncryptionMode Explicit -CertificatePolicy TrustOnFirstUse -KnownCertificatesPath '.\ftps-known-certificates.tsv'</code>
/// </example>
/// <example>
///   <para>Reuse a FluentFTP profile and still override proxy/runtime settings for the session.</para>
///   <code>$ftp = Connect-FTP -FtpProfile $profile -ProxyType UserAtHost -ProxyHost 'proxy.example.com' -ProxyPort 21 -ConnectTimeout 15000</code>
/// </example>
/// </summary>
[Cmdlet("Connect", "FTP", DefaultParameterSetName = "Password")]
public sealed class CmdletConnectFtp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the proxy Host.
	/// </summary>
	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? ProxyHost { get; set; }
	/// <summary>
	/// Gets or sets the proxy Port.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int ProxyPort { get; set; }
	/// <summary>
	/// Gets or sets the credential used by the cmdlet.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public PSCredential? ProxyCredential { get; set; }
	/// <summary>
	/// Gets or sets the proxy User Name.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? ProxyUserName { get; set; }
	/// <summary>
	/// Gets or sets the proxy Password.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? ProxyPassword { get; set; }
	/// <summary>
	/// Gets or sets the proxy Type.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public TransferettoFtpProxyType? ProxyType { get; set; }
	/// <summary>
	/// Gets or sets the ftp Profile.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	public FtpProfile? FtpProfile { get; set; }
	/// <summary>
	/// Gets or sets the server name or address.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? Server { get; set; }
	/// <summary>
	/// Gets or sets the username.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	public string? Username { get; set; }
	/// <summary>
	/// Gets or sets the password.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	public string? Password { get; set; }
	/// <summary>
	/// Gets or sets the credential used by the cmdlet.
	/// </summary>

	[Parameter(ParameterSetName = "Password")]
	public PSCredential? Credential { get; set; }
	/// <summary>
	/// Gets or sets the encryption Mode.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpEncryptionMode[]? EncryptionMode { get; set; }
	/// <summary>
	/// Gets or sets the data Connection Type.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpDataConnectionType DataConnectionType { get; set; }
	/// <summary>
	/// Gets or sets the ssl Buffering.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpsBuffering SslBuffering { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether disable Data Connection Encryption.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter DisableDataConnectionEncryption { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether disable Validate Certificate Revocation.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter DisableValidateCertificateRevocation { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether validate Any Certificate.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter ValidateAnyCertificate { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether the FluentFTP GnuTLS stream should be used for FTPS connections.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter UseGnuTls { get; set; }
	/// <summary>
	/// Gets or sets expected FTPS certificate thumbprints.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string[]? ExpectedCertificateThumbprint { get; set; }
	/// <summary>
	/// Gets or sets the FTPS certificate validation policy.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public TransferettoFtpCertificatePolicy CertificatePolicy { get; set; } = TransferettoFtpCertificatePolicy.PolicyChain;
	/// <summary>
	/// Gets or sets the known-certificate store path.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? KnownCertificatesPath { get; set; }
	/// <summary>
	/// Gets or sets the FTP control connection timeout, in milliseconds.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int ConnectTimeout { get; set; }
	/// <summary>
	/// Gets or sets the FTP control socket read timeout, in milliseconds.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int ReadTimeout { get; set; }
	/// <summary>
	/// Gets or sets the FTP data connection timeout, in milliseconds.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int DataConnectionConnectTimeout { get; set; }
	/// <summary>
	/// Gets or sets the FTP data socket read timeout, in milliseconds.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int DataConnectionReadTimeout { get; set; }
	/// <summary>
	/// Gets or sets the FTP control connection NOOP interval, in milliseconds. Set to 0 to disable NOOPs.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int NoopInterval { get; set; }
	/// <summary>
	/// Gets or sets the maximum number of FTPS control-channel transactions before the client reconnects.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int SslSessionLength { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether the CCC command should be used after authentication.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter EncryptAuthenticationOnly { get; set; }
	/// <summary>
	/// Gets or sets how FluentFTP should reconnect when it needs a control connection.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpSelfConnectMode SelfConnectMode { get; set; }
	/// <summary>
	/// Gets or sets the number of retry attempts for verified transfers.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int RetryAttempts { get; set; }
	/// <summary>
	/// Gets or sets the number of bytes transferred in a single FTP transfer chunk.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int TransferChunkSize { get; set; }
	/// <summary>
	/// Gets or sets the local file buffer size used by FTP transfers.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int LocalFileBufferSize { get; set; }
	/// <summary>
	/// Gets or sets the internet protocol versions allowed for FTP connections.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpIpVersion InternetProtocolVersions { get; set; }
	/// <summary>
	/// Gets or sets the upload rate limit in kilobytes per second.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public uint UploadRateLimit { get; set; }
	/// <summary>
	/// Gets or sets the download rate limit in kilobytes per second.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public uint DownloadRateLimit { get; set; }
	/// <summary>
	/// Gets or sets the data type used by high-level FTP uploads.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpDataType UploadDataType { get; set; }
	/// <summary>
	/// Gets or sets the data type used by high-level FTP downloads.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpDataType DownloadDataType { get; set; }
	/// <summary>
	/// Gets or sets the data type used by FTP directory listings.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpDataType ListingDataType { get; set; }
	/// <summary>
	/// Gets or sets the data type used by FXP server-to-server transfers.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public FtpDataType FXPDataType { get; set; }
	/// <summary>
	/// Gets or sets how often FXP progress is reported.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int FXPProgressInterval { get; set; }
	/// <summary>
	/// Gets or sets active-mode data ports.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int[]? ActivePorts { get; set; }
	/// <summary>
	/// Gets or sets passive-mode ports to avoid.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int[]? PassiveBlockedPorts { get; set; }
	/// <summary>
	/// Gets or sets the maximum number of passive-mode connection attempts.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int PassiveMaxAttempts { get; set; }
	/// <summary>
	/// Gets or sets the text encoding name used by the FTP control channel.
	/// </summary>

	[Parameter(ParameterSetName = "FtpProfile")]
	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public string? EncodingName { get; set; }
	/// <summary>
	/// Gets or sets the network port.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int Port { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether send Host.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter SendHost { get; set; }
	/// <summary>
	/// Gets or sets the socket Keep Alive.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter SocketKeepAlive { get; set; }
	/// <summary>
	/// Gets or sets the auto Connect.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter AutoConnect { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		try
		{
			TransferettoFtpConnectionOptions options = new TransferettoFtpConnectionOptions
			{
				Server = Server,
				Port = ((Port > 0) ? new int?(Port) : ((int?)null)),
				Credential = ResolveCredential(),
				FtpProfile = FtpProfile,
				EncryptionMode = EncryptionMode,
				DataConnectionType = (base.MyInvocation.BoundParameters.ContainsKey("DataConnectionType") ? new FtpDataConnectionType?(DataConnectionType) : ((FtpDataConnectionType?)null)),
				SslBuffering = (base.MyInvocation.BoundParameters.ContainsKey("SslBuffering") ? new FtpsBuffering?(SslBuffering) : ((FtpsBuffering?)null)),
				DisableDataConnectionEncryption = DisableDataConnectionEncryption.IsPresent,
				DisableValidateCertificateRevocation = DisableValidateCertificateRevocation.IsPresent,
				ValidateAnyCertificate = ValidateAnyCertificate.IsPresent,
				UseGnuTls = UseGnuTls.IsPresent,
				ExpectedCertificateThumbprints = ExpectedCertificateThumbprint,
				CertificatePolicy = CertificatePolicy,
				KnownCertificatesPath = KnownCertificatesPath,
				ConnectTimeout = (base.MyInvocation.BoundParameters.ContainsKey("ConnectTimeout") ? new int?(ConnectTimeout) : ((int?)null)),
				ReadTimeout = (base.MyInvocation.BoundParameters.ContainsKey("ReadTimeout") ? new int?(ReadTimeout) : ((int?)null)),
				DataConnectionConnectTimeout = (base.MyInvocation.BoundParameters.ContainsKey("DataConnectionConnectTimeout") ? new int?(DataConnectionConnectTimeout) : ((int?)null)),
				DataConnectionReadTimeout = (base.MyInvocation.BoundParameters.ContainsKey("DataConnectionReadTimeout") ? new int?(DataConnectionReadTimeout) : ((int?)null)),
				NoopInterval = (base.MyInvocation.BoundParameters.ContainsKey("NoopInterval") ? new int?(NoopInterval) : ((int?)null)),
				SslSessionLength = (base.MyInvocation.BoundParameters.ContainsKey("SslSessionLength") ? new int?(SslSessionLength) : ((int?)null)),
				EncryptAuthenticationOnly = EncryptAuthenticationOnly.IsPresent,
				SelfConnectMode = (base.MyInvocation.BoundParameters.ContainsKey("SelfConnectMode") ? new FtpSelfConnectMode?(SelfConnectMode) : ((FtpSelfConnectMode?)null)),
				RetryAttempts = (base.MyInvocation.BoundParameters.ContainsKey("RetryAttempts") ? new int?(RetryAttempts) : ((int?)null)),
				TransferChunkSize = (base.MyInvocation.BoundParameters.ContainsKey("TransferChunkSize") ? new int?(TransferChunkSize) : ((int?)null)),
				LocalFileBufferSize = (base.MyInvocation.BoundParameters.ContainsKey("LocalFileBufferSize") ? new int?(LocalFileBufferSize) : ((int?)null)),
				InternetProtocolVersions = (base.MyInvocation.BoundParameters.ContainsKey("InternetProtocolVersions") ? new FtpIpVersion?(InternetProtocolVersions) : ((FtpIpVersion?)null)),
				UploadRateLimit = (base.MyInvocation.BoundParameters.ContainsKey("UploadRateLimit") ? new uint?(UploadRateLimit) : ((uint?)null)),
				DownloadRateLimit = (base.MyInvocation.BoundParameters.ContainsKey("DownloadRateLimit") ? new uint?(DownloadRateLimit) : ((uint?)null)),
				UploadDataType = (base.MyInvocation.BoundParameters.ContainsKey("UploadDataType") ? new FtpDataType?(UploadDataType) : ((FtpDataType?)null)),
				DownloadDataType = (base.MyInvocation.BoundParameters.ContainsKey("DownloadDataType") ? new FtpDataType?(DownloadDataType) : ((FtpDataType?)null)),
				ListingDataType = (base.MyInvocation.BoundParameters.ContainsKey("ListingDataType") ? new FtpDataType?(ListingDataType) : ((FtpDataType?)null)),
				FXPDataType = (base.MyInvocation.BoundParameters.ContainsKey("FXPDataType") ? new FtpDataType?(FXPDataType) : ((FtpDataType?)null)),
				FXPProgressInterval = (base.MyInvocation.BoundParameters.ContainsKey("FXPProgressInterval") ? new int?(FXPProgressInterval) : ((int?)null)),
				ActivePorts = ActivePorts,
				PassiveBlockedPorts = PassiveBlockedPorts,
				PassiveMaxAttempts = (base.MyInvocation.BoundParameters.ContainsKey("PassiveMaxAttempts") ? new int?(PassiveMaxAttempts) : ((int?)null)),
				EncodingName = EncodingName,
				SendHost = SendHost.IsPresent,
				SocketKeepAlive = SocketKeepAlive.IsPresent,
				AutoConnect = AutoConnect.IsPresent,
				ProxyOptions = ResolveProxyOptions()
			};
			TransferettoFtpSession transferettoFtpSession = TransferettoClient.ConnectFtp(options);
			if (transferettoFtpSession.AutoDetectedProfile != null && AutoConnect.IsPresent)
			{
				WriteVerbose("Following options were used to autoconnect:");
				foreach (PSPropertyInfo property in PSObject.AsPSObject(transferettoFtpSession.AutoDetectedProfile).Properties)
				{
					WriteVerbose($"[x] {property.Name} -> {property.Value}");
				}
			}
			WriteObject(transferettoFtpSession);
		}
		catch (Exception exception)
		{
			object? obj = Server;
			if (obj == null)
			{
				obj = FtpProfile;
			}
			if (obj == null)
			{
				obj = base.ParameterSetName;
			}
			WriteError(new ErrorRecord(exception, "ConnectFtpFailed", ErrorCategory.ConnectionError, obj));
		}
	}

	private NetworkCredential? ResolveCredential()
	{
		if (!string.IsNullOrWhiteSpace(Username) && Password != null)
		{
			return new NetworkCredential(Username, Password);
		}
		if (Credential != null)
		{
			return Credential.GetNetworkCredential();
		}
		return null;
	}

	private TransferettoFtpProxyOptions? ResolveProxyOptions()
	{
		if (string.IsNullOrWhiteSpace(ProxyHost) && ProxyPort <= 0 && !ProxyType.HasValue)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(ProxyHost) || !ProxyType.HasValue)
		{
			throw new PSArgumentException("ProxyHost and ProxyType must be specified together when using proxy options.");
		}
		return new TransferettoFtpProxyOptions
		{
			ProxyHost = (ProxyHost ?? string.Empty),
			ProxyPort = ProxyPort,
			ProxyCredential = ResolveProxyCredential(),
			ProxyType = ProxyType.Value
		};
	}

	private NetworkCredential? ResolveProxyCredential()
	{
		if (!string.IsNullOrWhiteSpace(ProxyUserName) && ProxyPassword != null)
		{
			return new NetworkCredential(ProxyUserName, ProxyPassword);
		}
		return ProxyCredential?.GetNetworkCredential();
	}
}
