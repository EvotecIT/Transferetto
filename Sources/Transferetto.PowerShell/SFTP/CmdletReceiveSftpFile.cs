using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Downloads a file from an SFTP session to the local machine.</para>
/// <para type="description">Uses the shared async transfer pipeline so SFTP downloads support cancellation and progress reporting consistently with the FTP, SCP, and broader Transferetto file-transfer surface.</para>
/// <example>
///   <para>Download a log file from a Linux server with progress reporting.</para>
///   <code>Receive-SFTPFile -SftpClient $sftp -RemotePath '/var/log/app.log' -LocalPath '.\logs\app.log' -ShowProgress</code>
/// </example>
/// <example>
///   <para>Back up a remote configuration file before making changes.</para>
///   <code>Receive-SFTPFile -SftpClient $sftp -RemotePath '/etc/nginx/nginx.conf' -LocalPath '.\backup\nginx.conf'</code>
/// </example>
/// </summary>
[Alias(new string[] { "Get-SFTPFile" })]
[Cmdlet("Receive", "SFTPFile")]
public sealed class CmdletReceiveSftpFile : AsyncPSCmdlet {
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
			TransferettoTransferResult result = await TransferettoClient.DownloadSftpFileAsync(
				SftpClient,
				RemotePath!,
				LocalPath!,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveSftpFileFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
