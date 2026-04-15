using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SFTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Add-SFTPDirectory" })]
[Cmdlet("Send", "SFTPDirectory")]
public sealed class CmdletSendSftpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
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
		if (SftpClient == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = TransferettoClient.UploadSftpDirectory(SftpClient, LocalPath!, RemotePath!, AllowOverride.IsPresent);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline, enumerateCollection: true);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSftpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

