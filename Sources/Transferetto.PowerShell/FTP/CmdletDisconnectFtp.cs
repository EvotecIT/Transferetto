using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Disconnects an FTP or FTPS session.</para>
/// <para type="description">Closes the reusable FTP session created by <c>Connect-FTP</c> so sockets and authentication state are released cleanly at the end of a script or pipeline.</para>
/// <example>
///   <para>Close the current FTP session.</para>
///   <code>Disconnect-FTP -Client $ftp</code>
/// </example>
/// </summary>

[Cmdlet("Disconnect", "FTP")]
public sealed class CmdletDisconnectFtp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(ValueFromPipeline = true)]
	public TransferettoFtpSession? Client { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectFtp(Client);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectFtpFailed", ErrorCategory.CloseError, Client.Host));
		}
	}
}
