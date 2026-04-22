using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Downloads a remote directory tree through an SCP session.</para>
/// <para type="description">Provides recursive SCP downloads with the same shared progress reporting and cancellation-aware async behavior used by the rest of the Transferetto file-transfer surface.</para>
/// <example>
///   <para>Download a remote release directory from a Linux server.</para>
///   <code>Receive-SCPDirectory -ScpClient $scp -RemotePath '/srv/releases/current' -LocalPath '.\releases\current'</code>
/// </example>
/// <example>
///   <para>Download a remote directory and stream progress to the pipeline host.</para>
///   <code>Receive-SCPDirectory -ScpClient $scp -RemotePath '/var/log' -LocalPath '.\logs' -ShowProgress</code>
/// </example>
/// </summary>

[Alias(new string[] { "Get-SCPDirectory" })]
[Cmdlet("Receive", "SCPDirectory")]
public sealed class CmdletReceiveScpDirectory : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoScpSession? ScpClient { get; set; }
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
		if (ScpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			TransferettoTransferResult result = await TransferettoClient.DownloadScpDirectoryAsync(
				ScpClient,
				RemotePath!,
				LocalPath!,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveScpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
