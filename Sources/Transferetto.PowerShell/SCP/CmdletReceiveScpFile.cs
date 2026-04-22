using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Downloads a file through an SCP session.</para>
/// <para type="description">Provides a simple SCP receive path with the shared Transferetto async transfer options so scripts can show progress and cancel long downloads consistently.</para>
/// <example>
///   <para>Download a deployment archive from a Linux server.</para>
///   <code>Receive-SCPFile -ScpClient $scp -RemotePath '/var/www/app.tar.gz' -LocalPath '.\app.tar.gz' -ShowProgress</code>
/// </example>
/// <example>
///   <para>Copy a remote backup file to the current machine for offline inspection.</para>
///   <code>Receive-SCPFile -ScpClient $scp -RemotePath '/srv/backups/nightly.tar.gz' -LocalPath '.\nightly.tar.gz'</code>
/// </example>
/// </summary>
[Alias(new string[] { "Get-SCPFile" })]
[Cmdlet("Receive", "SCPFile")]
public sealed class CmdletReceiveScpFile : AsyncPSCmdlet {
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
			TransferettoTransferResult result = await TransferettoClient.DownloadScpFileAsync(
				ScpClient,
				RemotePath!,
				LocalPath!,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveScpFileFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
