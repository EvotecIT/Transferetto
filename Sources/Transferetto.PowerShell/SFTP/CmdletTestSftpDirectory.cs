using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Checks whether a remote SFTP directory exists.</para>
/// <para type="description">Returns a Boolean-like existence result for a remote SFTP directory path, which is useful before create, remove, or sync operations.</para>
/// <example>
///   <para>Check whether a remote directory exists.</para>
///   <code>Test-SFTPDirectory -SftpClient $sftp -Path '/srv/releases'</code>
/// </example>
/// </summary>

[Cmdlet("Test", "SFTPDirectory")]
public sealed class CmdletTestSftpDirectory : PSCmdlet
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.TestSftpDirectory(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestSftpDirectoryFailed", ErrorCategory.ReadError, Path));
		}
	}
}
