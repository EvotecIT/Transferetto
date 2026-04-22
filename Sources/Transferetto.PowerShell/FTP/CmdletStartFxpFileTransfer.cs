using System;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Start-FXPFileTransfer cmdlet.
/// </summary>

[Alias(new string[] { "Start-FXPFile" })]
[Cmdlet("Start", "FXPFileTransfer")]
public sealed class CmdletStartFxpFileTransfer : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Alias(new string[] { "SourceClient" })]
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the source Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? SourcePath { get; set; }
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>

	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? DestinationClient { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }
	/// <summary>
	/// Gets or sets the create Remote Directory.
	/// </summary>

	[Parameter]
	public SwitchParameter CreateRemoteDirectory { get; set; }
	/// <summary>
	/// Gets or sets the remote Exists.
	/// </summary>

	[Parameter]
	public FtpRemoteExists RemoteExists { get; set; } = FtpRemoteExists.Skip;
	/// <summary>
	/// Gets or sets the verify Options.
	/// </summary>

	[Parameter]
	public FtpVerify[]? VerifyOptions { get; set; }
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
		if (Client == null || DestinationClient == null || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			TransferettoTransferResult result = await TransferettoClient.StartFxpFileTransferAsync(
				Client,
				SourcePath!,
				DestinationClient,
				DestinationPath!,
				CreateRemoteDirectory.IsPresent,
				RemoteExists,
				VerifyOptions.CombineFlags(),
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "StartFxpFileTransferFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}
