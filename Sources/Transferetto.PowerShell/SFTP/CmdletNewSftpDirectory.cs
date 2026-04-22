using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates a directory on an SFTP server.</para>
/// <para type="description">Creates a remote SFTP directory and returns the operation result unless output is suppressed, making it easy to compose idempotent provisioning flows.</para>
/// <example>
///   <para>Create a remote SFTP directory.</para>
///   <code>New-SFTPDirectory -SftpClient $sftp -Path '/srv/uploads'</code>
/// </example>
/// <example>
///   <para>Create a directory but keep the pipeline quiet in a larger script.</para>
///   <code>New-SFTPDirectory -SftpClient $sftp -Path '/srv/releases/next' -Suppress</code>
/// </example>
/// </summary>

[Cmdlet("New", "SFTPDirectory")]
public sealed class CmdletNewSftpDirectory : PSCmdlet
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
			TransferettoOperationResult sendToPipeline = TransferettoClient.CreateSftpDirectory(SftpClient, Path!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "NewSftpDirectoryFailed", ErrorCategory.WriteError, Path));
		}
	}
}
