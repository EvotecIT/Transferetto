using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Move-SFTPFile cmdlet.
/// </summary>

[Cmdlet("Move", "SFTPFile")]
public sealed class CmdletMoveSftpFile : PSCmdlet
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
			TransferettoOperationResult sendToPipeline = TransferettoClient.MoveSftpFile(SftpClient, SourcePath!, DestinationPath!, PosixRename.IsPresent);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "MoveSftpFileFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}

