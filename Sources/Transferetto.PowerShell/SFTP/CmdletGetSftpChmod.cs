using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads POSIX-style permission bits for a remote SFTP item.</para>
/// <para type="description">Returns the remote mode/permission information reported by the SFTP server so scripts can inspect Unix-style access flags before applying changes.</para>
/// <example>
///   <para>Read permissions for a remote script.</para>
///   <code>Get-SFTPChmod -SftpClient $sftp -Path '/srv/app/deploy.sh'</code>
/// </example>
/// </summary>

[Cmdlet("Get", "SFTPChmod")]
public sealed class CmdletGetSftpChmod : PSCmdlet
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
			WriteObject(TransferettoClient.GetSftpChmod(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpChmodFailed", ErrorCategory.ReadError, Path));
		}
	}
}
