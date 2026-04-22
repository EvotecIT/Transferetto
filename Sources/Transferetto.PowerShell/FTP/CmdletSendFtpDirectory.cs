using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Uploads a local directory tree to an FTP or FTPS session.</para>
/// <para type="description">Supports FluentFTP folder sync modes, remote collision policy, verification, transfer rules, shared progress reporting, and cancellation-aware async directory uploads for both FTP and FTPS targets.</para>
/// <example>
///   <para>Upload a local website directory and update changed files on the server.</para>
///   <code>Send-FTPDirectory -Client $ftp -LocalPath '.\Website' -RemotePath '/wwwroot' -FolderSyncMode Update -ShowProgress</code>
/// </example>
/// <example>
///   <para>Upload build artifacts and overwrite existing remote files when names collide.</para>
///   <code>Send-FTPDirectory -Client $ftp -LocalPath '.\Artifacts' -RemotePath '/incoming' -RemoteExists Overwrite</code>
/// </example>
/// <example>
///   <para>Apply transfer rules during a recursive upload.</para>
///   <code>Send-FTPDirectory -Client $ftp -LocalPath '.\Website' -RemotePath '/wwwroot' -Rules $rules -VerifyOptions Retry</code>
/// </example>
/// </summary>
[Alias(new string[] { "Add-FTPDirectory" })]
[Cmdlet("Send", "FTPDirectory")]
public sealed class CmdletSendFtpDirectory : AsyncPSCmdlet {
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
			IReadOnlyList<TransferettoTransferResult> result = await TransferettoClient.UploadFtpDirectoryAsync(
				Client,
				LocalPath!,
				RemotePath!,
				FolderSyncMode,
				RemoteExists,
				VerifyOptions,
				Rules,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(result, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "SendFtpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
