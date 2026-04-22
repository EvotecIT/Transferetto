using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Checks whether a remote SFTP path exists.</para>
/// <para type="description">Returns a Boolean-like existence result for a remote SFTP path regardless of whether it is a file, directory, or other supported item type.</para>
/// <example>
///   <para>Check whether a remote path exists.</para>
///   <code>Test-SFTPPath -SftpClient $sftp -Path '/srv/app/current'</code>
/// </example>
/// </summary>

[Cmdlet("Test", "SFTPPath")]
public sealed class CmdletTestSftpPath : PSCmdlet
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
			WriteObject(TransferettoClient.TestSftpPath(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestSftpPathFailed", ErrorCategory.ReadError, Path));
		}
	}
}
