using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-SFTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Get-SFTPDirectory" })]
[Cmdlet("Receive", "SFTPDirectory")]
public sealed class CmdletReceiveSftpDirectory : AsyncPSCmdlet {
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
		if (SftpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoTransferResult> result = await TransferettoClient.DownloadSftpDirectoryAsync(
				SftpClient,
				RemotePath!,
				LocalPath!,
				AllowOverride.IsPresent,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveSftpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
