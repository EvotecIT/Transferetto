using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Downloads a remote FTP or FTPS directory tree to the local machine.</para>
/// <para type="description">Supports FluentFTP folder sync modes, local collision policy, optional verification rules, shared progress reporting, and cancellation-aware async directory downloads for both FTP and FTPS sessions.</para>
/// <example>
///   <para>Download a remote folder into the current local working directory.</para>
///   <code>Receive-FTPDirectory -Client $ftp -RemotePath '/pub/example' -LocalPath '.\example'</code>
/// </example>
/// <example>
///   <para>Mirror a remote tree locally while removing files that no longer exist remotely.</para>
///   <code>Receive-FTPDirectory -Client $ftp -RemotePath '/webroot' -LocalPath '.\mirror\webroot' -FolderSyncMode Mirror -LocalExists Overwrite</code>
/// </example>
/// <example>
///   <para>Download a directory with progress reporting and selective transfer rules.</para>
///   <code>Receive-FTPDirectory -Client $ftp -RemotePath '/logs' -LocalPath '.\logs' -Rules $rules -ShowProgress</code>
/// </example>
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
