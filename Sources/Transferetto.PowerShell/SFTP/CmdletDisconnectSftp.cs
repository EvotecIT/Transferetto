using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Disconnect-SFTP cmdlet.
/// </summary>

[Cmdlet("Disconnect", "SFTP")]
public sealed class CmdletDisconnectSftp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSftpSession? SftpClient { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectSftp(SftpClient);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectSftpFailed", ErrorCategory.CloseError, SftpClient.Host));
		}
	}
}

