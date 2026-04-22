using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates a symbolic link on an SFTP server.</para>
/// <para type="description">Creates a remote symlink from the requested link path to the target path and can suppress the returned operation result for quieter automation.</para>
/// <example>
///   <para>Create a current-release symlink.</para>
///   <code>New-SFTPSymbolicLink -SftpClient $sftp -TargetPath '/srv/releases/2026-04-22' -LinkPath '/srv/releases/current'</code>
/// </example>
/// </summary>

[Cmdlet("New", "SFTPSymbolicLink")]
public sealed class CmdletNewSftpSymbolicLink : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the target Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? TargetPath { get; set; }
	/// <summary>
	/// Gets or sets the link Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LinkPath { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(TargetPath) || string.IsNullOrWhiteSpace(LinkPath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.CreateSftpSymbolicLink(SftpClient, TargetPath!, LinkPath!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "NewSftpSymbolicLinkFailed", ErrorCategory.WriteError, LinkPath));
		}
	}
}
