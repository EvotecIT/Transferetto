using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Disconnects an SFTP session.</para>
/// <para type="description">Closes the reusable SFTP session created by <c>Connect-SFTP</c> so the underlying SSH transport is released cleanly when file operations are done.</para>
/// <example>
///   <para>Close the current SFTP session.</para>
///   <code>Disconnect-SFTP -SftpClient $sftp</code>
/// </example>
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
