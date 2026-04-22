using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Checks whether a remote SFTP symbolic link exists.</para>
/// <para type="description">Returns a Boolean-like existence result specifically for a symbolic-link path so scripts can distinguish link-oriented workflows from file or directory checks.</para>
/// <example>
///   <para>Check whether a remote symlink exists.</para>
///   <code>Test-SFTPSymbolicLink -SftpClient $sftp -Path '/srv/releases/current'</code>
/// </example>
/// </summary>

[Cmdlet("Test", "SFTPSymbolicLink")]
public sealed class CmdletTestSftpSymbolicLink : PSCmdlet
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
			WriteObject(TransferettoClient.TestSftpSymbolicLink(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestSftpSymbolicLinkFailed", ErrorCategory.ReadError, Path));
		}
	}
}
