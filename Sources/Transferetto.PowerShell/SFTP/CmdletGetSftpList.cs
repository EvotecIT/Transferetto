using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Lists files and directories from an SFTP session.</para>
/// <para type="description">Returns Transferetto remote item objects for a target SFTP path so PowerShell scripts can inspect directory contents, filter entries, and pipe them into later file-management commands.</para>
/// <example>
///   <para>List the current SFTP working directory.</para>
///   <code>Get-SFTPList -SftpClient $sftp</code>
/// </example>
/// <example>
///   <para>List a specific remote folder and select a few useful fields.</para>
///   <code>Get-SFTPList -SftpClient $sftp -Path '/var/www/site' | Select-Object Name, FullName, Length</code>
/// </example>
/// </summary>

[Cmdlet("Get", "SFTPList")]
public sealed class CmdletGetSftpList : PSCmdlet
{
	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	[Alias(new string[] { "FtpPath" })]
	[Parameter]
	public string? Path { get; set; }
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
			WriteObject(TransferettoClient.GetSftpListing(SftpClient, Path!), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpListFailed", ErrorCategory.ReadError, Path ?? SftpClient.Host));
		}
	}
}
