using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Moves or renames a directory on an SFTP server.</para>
/// <para type="description">Renames or relocates a remote SFTP directory, optionally using POSIX rename semantics when the server supports them, and returns a structured operation result unless suppressed.</para>
/// <example>
///   <para>Rename a directory on the server.</para>
///   <code>Move-SFTPDirectory -SftpClient $sftp -SourcePath '/srv/releases/next' -DestinationPath '/srv/releases/current'</code>
/// </example>
/// </summary>

[Cmdlet("Move", "SFTPDirectory")]
public sealed class CmdletMoveSftpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the source Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? SourcePath { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }
	/// <summary>
	/// Gets or sets the posix Rename.
	/// </summary>

	[Parameter]
	public SwitchParameter PosixRename { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.MoveSftpDirectory(SftpClient, SourcePath!, DestinationPath!, PosixRename.IsPresent);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "MoveSftpDirectoryFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}
