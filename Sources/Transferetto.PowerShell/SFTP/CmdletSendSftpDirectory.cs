using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Uploads a local directory tree to an SFTP session.</para>
/// <para type="description">Uses the shared Transferetto async transfer pipeline so recursive SFTP uploads support overwrite control, progress reporting, and cancellation consistently with the FTP and SCP directory transfer cmdlets.</para>
/// <example>
///   <para>Upload a local directory to a remote SFTP path.</para>
///   <code>Send-SFTPDirectory -SftpClient $sftp -LocalPath '.\site' -RemotePath '/var/www/site'</code>
/// </example>
/// <example>
///   <para>Redeploy a directory tree and overwrite existing remote files while streaming progress.</para>
///   <code>Send-SFTPDirectory -SftpClient $sftp -LocalPath '.\publish' -RemotePath '/srv/releases/current' -AllowOverride -ShowProgress</code>
/// </example>
/// </summary>

[Alias(new string[] { "Add-SFTPDirectory" })]
[Cmdlet("Send", "SFTPDirectory")]
public sealed class CmdletSendSftpDirectory : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
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
		if (SftpClient == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath)) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoTransferResult> result = await TransferettoClient.UploadSftpDirectoryAsync(
				SftpClient,
				LocalPath!,
				RemotePath!,
				AllowOverride.IsPresent,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "SendSftpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
