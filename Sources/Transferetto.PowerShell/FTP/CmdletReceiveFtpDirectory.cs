using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-FTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Get-FTPDirectory" })]
[Cmdlet("Receive", "FTPDirectory")]
public sealed class CmdletReceiveFtpDirectory : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the folder Sync Mode.
	/// </summary>

	[Parameter]
	public FtpFolderSyncMode FolderSyncMode { get; set; } = FtpFolderSyncMode.Update;
	/// <summary>
	/// Gets or sets the local exists behavior.
	/// </summary>

	[Parameter]
	public FtpLocalExists LocalExists { get; set; } = FtpLocalExists.Skip;
	/// <summary>
	/// Gets or sets the verify options.
	/// </summary>

	[Parameter]
	public FtpVerify VerifyOptions { get; set; } = FtpVerify.None;
	/// <summary>
	/// Gets or sets the rules.
	/// </summary>

	[Parameter]
	public FtpRule[]? Rules { get; set; }
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
	protected override async Task ProcessRecordAsync() {
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoTransferResult> result = await TransferettoClient.DownloadFtpDirectoryAsync(
				Client,
				LocalPath!,
				RemotePath!,
				FolderSyncMode,
				LocalExists,
				VerifyOptions,
				Rules,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveFtpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
