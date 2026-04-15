using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Rename-SFTPFile cmdlet.
/// </summary>

[Cmdlet("Rename", "SFTPFile")]
public sealed class CmdletRenameSftpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the source Path.
	/// </summary>

	[Alias(new string[] { "OldPath" })]
	[Parameter]
	public string? SourcePath { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Alias(new string[] { "NewPath" })]
	[Parameter]
	public string? DestinationPath { get; set; }
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
			TransferettoOperationResult sendToPipeline = TransferettoClient.RenameSftpFile(SftpClient, SourcePath!, DestinationPath!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RenameSftpFileFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}

