using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Removes a file from an SFTP server.</para>
/// <para type="description">Deletes a single remote SFTP file and can suppress the returned operation result when used inside larger maintenance scripts.</para>
/// <example>
///   <para>Delete a remote SFTP file.</para>
///   <code>Remove-SFTPFile -SftpClient $sftp -RemotePath '/srv/app/old-config.json'</code>
/// </example>
/// </summary>

[Cmdlet("Remove", "SFTPFile")]
public sealed class CmdletRemoveSftpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.RemoveSftpFile(SftpClient, RemotePath!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RemoveSftpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
