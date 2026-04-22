using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Removes a directory from an SFTP server.</para>
/// <para type="description">Deletes a remote SFTP directory and returns the operation result unless suppressed, which fits well into cleanup and deployment-rollback scripts.</para>
/// <example>
///   <para>Remove a remote SFTP directory.</para>
///   <code>Remove-SFTPDirectory -SftpClient $sftp -Path '/srv/releases/old'</code>
/// </example>
/// <example>
///   <para>Delete a directory quietly inside a larger maintenance script.</para>
///   <code>Remove-SFTPDirectory -SftpClient $sftp -Path '/srv/tmp/build-cache' -Suppress</code>
/// </example>
/// </summary>

[Cmdlet("Remove", "SFTPDirectory")]
public sealed class CmdletRemoveSftpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.RemoveSftpDirectory(SftpClient, Path!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RemoveSftpDirectoryFailed", ErrorCategory.WriteError, Path));
		}
	}
}
