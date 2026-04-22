using System;
using System.Management.Automation;
using System.Net;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Probes an FTP or FTPS endpoint to discover compatible connection settings.</para>
/// <para type="description">Runs Transferetto’s FTP configuration detection against a target server, optionally with credentials, and can return either the first working configuration or the full candidate set.</para>
/// <example>
///   <para>Probe a server anonymously or with default credentials.</para>
///   <code>Request-FTPConfiguration -Server 'ftp.example.com'</code>
/// </example>
/// <example>
///   <para>Probe with credentials and stop after the first successful configuration.</para>
///   <code>Request-FTPConfiguration -Server 'ftp.example.com' -Credential (Get-Credential) -FirstOnly</code>
/// </example>
/// </summary>

[Cmdlet("Request", "FTPConfiguration", DefaultParameterSetName = "Password")]
public sealed class CmdletRequestFtpConfiguration : PSCmdlet
{
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
	/// Gets or sets the network port.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public int Port { get; set; }
	/// <summary>
	/// Gets or sets the first Only.
	/// </summary>

	[Parameter(ParameterSetName = "ClearText")]
	[Parameter(ParameterSetName = "Password")]
	public SwitchParameter FirstOnly { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		try
		{
			TransferettoFtpConnectionOptions options = new TransferettoFtpConnectionOptions
			{
				Server = Server,
				Port = ((Port > 0) ? new int?(Port) : ((int?)null)),
				Credential = ResolveCredential()
			};
			WriteObject(TransferettoClient.DetectFtpConfiguration(options, FirstOnly.IsPresent), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RequestFtpConfigurationFailed", ErrorCategory.ConnectionError, Server));
		}
	}

	private NetworkCredential? ResolveCredential()
	{
		if (!string.IsNullOrWhiteSpace(Username) && Password != null)
		{
			return new NetworkCredential(Username, Password);
		}
		return Credential?.GetNetworkCredential();
	}
}
