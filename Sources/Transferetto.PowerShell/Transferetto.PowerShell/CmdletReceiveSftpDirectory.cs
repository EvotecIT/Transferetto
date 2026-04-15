using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-SFTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Get-SFTPDirectory" })]
[Cmdlet("Receive", "SFTPDirectory")]
public sealed class CmdletReceiveSftpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the allow Override.
	/// </summary>

	[Parameter]
	public SwitchParameter AllowOverride { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath))
		{
			return;
		}
		try
		{
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = TransferettoClient.DownloadSftpDirectory(SftpClient, RemotePath!, LocalPath!, AllowOverride.IsPresent);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline, enumerateCollection: true);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReceiveSftpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

