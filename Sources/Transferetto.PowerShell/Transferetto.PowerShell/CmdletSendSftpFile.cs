using System;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SFTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Add-SFTPFile" })]
[Cmdlet("Send", "SFTPFile")]
public sealed class CmdletSendSftpFile : PSCmdlet
{
	private readonly CancellationTokenSource cancellationTokenSource = new();
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
	/// <summary>
	/// Gets or sets a value indicating whether transfer progress is displayed.
	/// </summary>

	[Parameter]
	public SwitchParameter ShowProgress { get; set; }
	/// <summary>
	/// Gets or sets the minimum number of bytes between progress updates.
	/// </summary>

	[Parameter]
	public long ProgressIntervalBytes { get; set; } = 65536;

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
			TransferettoTransferOptions options = new()
			{
				CancellationToken = cancellationTokenSource.Token,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new CmdletTransferProgress(this) : null
			};
			WriteObject(TransferettoClient.UploadSftpFile(SftpClient, LocalPath!, RemotePath!, AllowOverride.IsPresent, options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSftpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}

	/// <inheritdoc/>
	protected override void StopProcessing()
	{
		cancellationTokenSource.Cancel();
		base.StopProcessing();
	}

	private sealed class CmdletTransferProgress : IProgress<TransferettoTransferProgress>
	{
		private readonly PSCmdlet cmdlet;

		public CmdletTransferProgress(PSCmdlet cmdlet)
		{
			this.cmdlet = cmdlet;
		}

		public void Report(TransferettoTransferProgress value)
		{
			int percentComplete = value.PercentComplete ?? -1;
			string activity = $"{value.Protocol} {value.Direction}";
			string status = value.TotalBytes.HasValue
				? $"{value.BytesTransferred} of {value.TotalBytes.Value} bytes"
				: $"{value.BytesTransferred} bytes";
			cmdlet.WriteProgress(new ProgressRecord(0, activity, status)
			{
				PercentComplete = percentComplete,
				CurrentOperation = value.RemotePath ?? value.LocalPath
			});
		}
	}
}
