using System;
using System.IO;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SFTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Add-SFTPFile" })]
[Cmdlet("Send", "SFTPFile")]
public sealed class CmdletSendSftpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the allow Override.
	/// </summary>

	[Parameter]
	public SwitchParameter AllowOverride { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath))
		{
			return;
		}
		if (!File.Exists(LocalPath))
		{
			WriteObject(new TransferettoTransferResult
			{
				Action = "UploadFile",
				Status = false,
				IsSuccess = false,
				IsSkipped = false,
				IsSkippedByRule = false,
				IsFailed = true,
				LocalPath = LocalPath,
				RemotePath = RemotePath,
				Message = "LocalPath doesn't exists " + LocalPath
			});
			return;
		}
		try
		{
			WriteObject(TransferettoClient.UploadSftpFile(SftpClient, LocalPath!, RemotePath!, AllowOverride.IsPresent));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSftpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

