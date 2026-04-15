using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Connect-SFTP cmdlet.
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
				Port = ((Port > 0) ? new int?(Port) : ((int?)null))
			};
			WriteObject(TransferettoClient.ConnectSftp(options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ConnectSftpFailed", ErrorCategory.ConnectionError, Server));
		}
	}
}

