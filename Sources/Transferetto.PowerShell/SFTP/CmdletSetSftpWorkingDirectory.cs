using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Changes the working directory for an SFTP session.</para>
/// <para type="description">Updates the session’s active SFTP path so later relative operations run against the intended remote location.</para>
/// <example>
///   <para>Change the remote SFTP working directory.</para>
///   <code>Set-SFTPWorkingDirectory -SftpClient $sftp -Path '/srv/app/releases'</code>
/// </example>
/// <example>
///   <para>Change the directory and continue with the same session object.</para>
///   <code>$sftp = Set-SFTPWorkingDirectory -SftpClient $sftp -Path '/srv/app/releases' -PassThru</code>
/// </example>
/// </summary>

[Cmdlet("Set", "SFTPWorkingDirectory")]
public sealed class CmdletSetSftpWorkingDirectory : PSCmdlet
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
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoClient.SetSftpWorkingDirectory(SftpClient, Path!);
			if (PassThru.IsPresent)
			{
				WriteObject(SftpClient);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpWorkingDirectoryFailed", ErrorCategory.InvalidOperation, Path));
		}
	}
}
