using System;
using System.Management.Automation;
using System.Net;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Connect-FTP cmdlet.
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

