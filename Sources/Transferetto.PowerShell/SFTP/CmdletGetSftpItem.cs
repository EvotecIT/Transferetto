using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Retrieves metadata for a single SFTP file-system item.</para>
/// <para type="description">Returns SFTP attributes for a target path so scripts can inspect timestamps, permissions, size, and item type before taking further action.</para>
/// <example>
///   <para>Inspect a remote SFTP file.</para>
///   <code>Get-SFTPItem -SftpClient $sftp -Path '/srv/app/settings.json'</code>
/// </example>
/// <example>
///   <para>Inspect a remote directory before deciding whether to create or remove children.</para>
///   <code>Get-SFTPItem -SftpClient $sftp -Path '/srv/releases'</code>
/// </example>
/// </summary>

[Cmdlet("Get", "SFTPItem")]
public sealed class CmdletGetSftpItem : PSCmdlet
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
			WriteObject(TransferettoClient.GetSftpAttributes(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpItemFailed", ErrorCategory.ReadError, Path));
		}
	}
}
