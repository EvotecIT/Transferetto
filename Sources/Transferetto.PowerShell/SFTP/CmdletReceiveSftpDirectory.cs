using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Downloads a remote directory tree from an SFTP session.</para>
/// <para type="description">Supports recursive SFTP downloads with overwrite control, progress reporting, and cancellation-aware async execution so local staging and backup workflows behave consistently across protocols.</para>
/// <example>
///   <para>Download a remote application directory to the local machine.</para>
///   <code>Receive-SFTPDirectory -SftpClient $sftp -RemotePath '/srv/app' -LocalPath '.\backup\app'</code>
/// </example>
/// <example>
///   <para>Refresh a local mirror from SFTP and overwrite existing files while showing progress.</para>
///   <code>Receive-SFTPDirectory -SftpClient $sftp -RemotePath '/var/log/nginx' -LocalPath '.\logs\nginx' -AllowOverride -ShowProgress</code>
/// </example>
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
