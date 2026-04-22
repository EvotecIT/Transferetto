using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates an SCP session with SSH host-key validation, proxy support, and password or private-key authentication.</para>
/// <para type="description">Uses the same SSH trust and authentication model as the SFTP and SSH cmdlets, making it easy to connect with credentials or private keys, enforce known-hosts or TOFU trust, and route SCP transfers through an SSH proxy when needed.</para>
/// <example>
///   <para>Connect to an SCP server with a credential prompt and trust-on-first-use validation.</para>
///   <code>$scp = Connect-SCP -Server 'test.rebex.net' -Credential (Get-Credential) -HostKeyPolicy TrustOnFirstUse</code>
/// </example>
/// <example>
///   <para>Connect with a private key and a reusable known-hosts file.</para>
///   <code>$scp = Connect-SCP -Server 'linux.example.com' -Username 'deploy' -PrivateKey '.\id_ed25519' -KnownHostsPath '.\ssh-known-hosts.tsv'</code>
/// </example>
/// <example>
///   <para>Connect through a SOCKS proxy for restricted network paths.</para>
///   <code>$scp = Connect-SCP -Server 'linux.example.com' -Credential (Get-Credential) -ProxyType Socks5 -ProxyHost 'proxy.example.com' -ProxyPort 1080</code>
/// </example>
/// </summary>
[Cmdlet("Connect", "SCP", DefaultParameterSetName = "Password")]
public sealed class CmdletConnectScp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the server name or address.
	/// </summary>
	[Parameter(Mandatory = true, ParameterSetName = "ClearText")]
	[Parameter(Mandatory = true, ParameterSetName = "Password")]
	[Parameter(Mandatory = true, ParameterSetName = "PrivateKey")]
	public string? Server { get; set; }
	/// <summary>
	/// Gets or sets the username.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ClearText")]
	[Parameter(Mandatory = true, ParameterSetName = "PrivateKey")]
	public string? Username { get; set; }
	/// <summary>
	/// Gets or sets the password.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ClearText")]
	public string? Password { get; set; }
	/// <summary>
	/// Gets or sets the credential used by the cmdlet.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Password")]
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
			TransferettoSshConnectionOptions options = new TransferettoSshConnectionOptions
			{
				Server = Server!,
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
			WriteObject(TransferettoClient.ConnectScp(options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ConnectScpFailed", ErrorCategory.ConnectionError, Server));
		}
	}
}
