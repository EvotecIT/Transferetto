using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-FTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Add-FTPFile" })]
[Cmdlet("Send", "FTPFile")]
public sealed class CmdletSendFtpFile : PSCmdlet
{
	private readonly CancellationTokenSource cancellationTokenSource = new();
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local File.
	/// </summary>

	[Parameter]
	public FileInfo[]? LocalFile { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter]
	public string[]? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Exists.
	/// </summary>

	[Parameter]
	public FtpRemoteExists RemoteExists { get; set; } = FtpRemoteExists.Skip;
	/// <summary>
	/// Gets or sets the verify Options.
	/// </summary>

	[Parameter]
	public FtpVerify VerifyOptions { get; set; } = FtpVerify.None;
	/// <summary>
	/// Gets or sets the error Handling.
	/// </summary>

	[Parameter]
	public FtpError ErrorHandling { get; set; } = FtpError.None;
	/// <summary>
	/// Gets or sets the create Remote Directory.
	/// </summary>

	[Parameter]
	public SwitchParameter CreateRemoteDirectory { get; set; }
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
		if (Client == null)
		{
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
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = TransferettoClient.UploadFtpFiles(Client, RemotePath!, LocalPath!, LocalFile, RemoteExists, VerifyOptions, ErrorHandling, CreateRemoteDirectory.IsPresent, options);
			WriteObject(sendToPipeline, enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendFtpFileFailed", ErrorCategory.WriteError, RemotePath));
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
