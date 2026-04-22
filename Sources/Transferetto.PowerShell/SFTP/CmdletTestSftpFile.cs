using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Checks whether a remote SFTP file exists.</para>
/// <para type="description">Returns a Boolean-like existence result for a remote SFTP file path, which is useful for guard clauses and idempotent deployment flows.</para>
/// <example>
///   <para>Check whether a remote file exists.</para>
///   <code>Test-SFTPFile -SftpClient $sftp -Path '/srv/app/config.json'</code>
/// </example>
/// </summary>

[Cmdlet("Test", "SFTPFile")]
public sealed class CmdletTestSftpFile : PSCmdlet
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
			WriteObject(TransferettoClient.TestSftpFile(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestSftpFileFailed", ErrorCategory.ReadError, Path));
		}
	}
}
