using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Returns the current working directory for an SFTP session.</para>
/// <para type="description">Exposes the session’s active SFTP path so scripts can coordinate relative reads, writes, and directory-management steps safely.</para>
/// <example>
///   <para>Show the current SFTP working directory.</para>
///   <code>Get-SFTPWorkingDirectory -SftpClient $sftp</code>
/// </example>
/// </summary>

[Cmdlet("Get", "SFTPWorkingDirectory")]
public sealed class CmdletGetSftpWorkingDirectory : PSCmdlet
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
			WriteObject(TransferettoClient.GetSftpWorkingDirectory(SftpClient));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpWorkingDirectoryFailed", ErrorCategory.ReadError, SftpClient.Host));
		}
	}
}
