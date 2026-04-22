using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SCPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Add-SCPDirectory" })]
[Cmdlet("Send", "SCPDirectory")]
public sealed class CmdletSendScpDirectory : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoScpSession? ScpClient { get; set; }
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
		if (ScpClient == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			TransferettoTransferResult result = await TransferettoClient.UploadScpDirectoryAsync(
				ScpClient,
				LocalPath!,
				RemotePath!,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "SendScpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
