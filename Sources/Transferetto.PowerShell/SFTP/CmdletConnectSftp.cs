using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates an SFTP session with SSH host-key trust, proxy support, and password or private-key authentication.</para>
/// <para type="description">Supports clear-text credentials, PSCredential objects, private keys, keyboard-interactive auth, trust-on-first-use and known-hosts validation, connection retries, keepalive settings, and SSH proxy configuration that can be reused by later SFTP cmdlets.</para>
/// <example>
///   <para>Connect to an SFTP server with an interactive credential prompt and trust-on-first-use host-key validation.</para>
///   <code>$sftp = Connect-SFTP -Server 'test.rebex.net' -Credential (Get-Credential) -HostKeyPolicy TrustOnFirstUse</code>
/// </example>
/// <example>
///   <para>Connect with a private key and persist trusted host keys to a reusable file.</para>
///   <code>$sftp = Connect-SFTP -Server 'sftp.example.com' -Username 'deploy' -PrivateKey '.\id_ed25519' -PrivateKeyPassphrase 'secret' -KnownHostsPath '.\ssh-known-hosts.tsv'</code>
/// </example>
/// <example>
///   <para>Connect through a SOCKS proxy with retry and keepalive tuning.</para>
///   <code>$sftp = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential) -ProxyType Socks5 -ProxyHost 'proxy.example.com' -ProxyPort 1080 -RetryAttempts 2 -KeepAliveIntervalSeconds 30</code>
/// </example>
/// </summary>
[Cmdlet("Connect", "SFTP", DefaultParameterSetName = "Password")]
public sealed class CmdletConnectSftp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the server name or address.
	/// </summary>
	[Parameter(ParameterSetName = "ClearText", Mandatory = true)]
	[Parameter(ParameterSetName = "Password", Mandatory = true)]
	[Parameter(ParameterSetName = "PrivateKey", Mandatory = true)]
	public string? Server { get; set; }
	/// <summary>
	/// Gets or sets the username.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText", Mandatory = true)]
	[Parameter(ParameterSetName = "PrivateKey", Mandatory = true)]
	public string? Username { get; set; }
	/// <summary>
	/// Gets or sets the password.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText", Mandatory = true)]
	public string? Password { get; set; }
	/// <summary>
	/// Gets or sets the credential used by the cmdlet.
	/// </summary>

	[Parameter(ParameterSetName = "Password", Mandatory = true)]
	public PSCredential? Credential { get; set; }
	/// <summary>
	/// Gets or sets the private Key.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "PrivateKey")]
	public string? PrivateKey { get; set; }
	/// <summary>
	/// Gets or sets the network port.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	[Parameter(ParameterSetName = "PrivateKey")]
	public int Port { get; set; }
	/// <summary>
	/// Gets or sets the private Key Passphrase.
	/// </summary>

	[Parameter]
	public string? PrivateKeyPassphrase { get; set; }
	/// <summary>
	/// Gets or sets the keyboard Interactive.
	/// </summary>

	[Parameter]
	public SwitchParameter KeyboardInteractive { get; set; }
	/// <summary>
	/// Gets or sets the accept Any Host Key.
	/// </summary>

	[Parameter]
	public SwitchParameter AcceptAnyHostKey { get; set; }
	/// <summary>
	/// Gets or sets the expected Host Key Fingerprint.
	/// </summary>

	[Alias(new string[] { "HostKeyFingerprint" })]
	[Parameter]
	public string[]? ExpectedHostKeyFingerprint { get; set; }
	/// <summary>
	/// Gets or sets the host Key Policy.
	/// </summary>

	[Parameter]
	public TransferettoSshHostKeyPolicy HostKeyPolicy { get; set; } = TransferettoSshHostKeyPolicy.TrustOnFirstUse;
	/// <summary>
	/// Gets or sets the known Hosts Path.
	/// </summary>

	[Alias(new string[] { "KnownHostsFile" })]
	[Parameter]
	public string? KnownHostsPath { get; set; }
	/// <summary>
	/// Gets or sets the keep Alive Interval Seconds.
	/// </summary>

	[Parameter]
	public int KeepAliveIntervalSeconds { get; set; }
	/// <summary>
	/// Gets or sets the connection Timeout Seconds.
	/// </summary>

	[Parameter]
	public int ConnectionTimeoutSeconds { get; set; }
	/// <summary>
	/// Gets or sets the retry Attempts.
	/// </summary>

	[Parameter]
	public int RetryAttempts { get; set; }
	/// <summary>
	/// Gets or sets the proxy Type.
	/// </summary>

	[Parameter]
	public TransferettoSshProxyType ProxyType { get; set; }
	/// <summary>
	/// Gets or sets the proxy Host.
	/// </summary>

	[Parameter]
	public string? ProxyHost { get; set; }
	/// <summary>
	/// Gets or sets the proxy Port.
	/// </summary>

	[Parameter]
	public int ProxyPort { get; set; }
	/// <summary>
	/// Gets or sets the credential used by the cmdlet.
	/// </summary>

	[Parameter]
	public PSCredential? ProxyCredential { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (string.IsNullOrWhiteSpace(Server))
		{
			return;
		}
		try
		{
			string server = Server!;
			TransferettoSftpConnectionOptions options = new TransferettoSftpConnectionOptions
			{
				Server = server,
				UserName = Username,
				Password = Password,
				Credential = Credential?.GetNetworkCredential(),
				PrivateKeyPath = PrivateKey,
				PrivateKeyPassphrase = PrivateKeyPassphrase,
				Port = ((Port > 0) ? new int?(Port) : ((int?)null)),
				EnableKeyboardInteractive = KeyboardInteractive.IsPresent,
				AcceptAnyHostKey = AcceptAnyHostKey.IsPresent,
				ExpectedHostKeyFingerprints = ExpectedHostKeyFingerprint,
				HostKeyPolicy = HostKeyPolicy,
				KnownHostsPath = KnownHostsPath,
				KeepAliveIntervalSeconds = (base.MyInvocation.BoundParameters.ContainsKey("KeepAliveIntervalSeconds") ? new int?(KeepAliveIntervalSeconds) : ((int?)null)),
				ConnectionTimeoutSeconds = (base.MyInvocation.BoundParameters.ContainsKey("ConnectionTimeoutSeconds") ? new int?(ConnectionTimeoutSeconds) : ((int?)null)),
				RetryAttempts = (base.MyInvocation.BoundParameters.ContainsKey("RetryAttempts") ? new int?(RetryAttempts) : ((int?)null)),
				ProxyType = ProxyType,
				ProxyHost = ProxyHost,
				ProxyPort = ((ProxyPort > 0) ? new int?(ProxyPort) : ((int?)null)),
				ProxyCredential = ProxyCredential?.GetNetworkCredential()
			};
			WriteObject(TransferettoClient.ConnectSftp(options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ConnectSftpFailed", ErrorCategory.ConnectionError, Server));
		}
	}
}
