using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Uploads one or more local files to an FTP or FTPS session.</para>
/// <para type="description">Supports explicit remote targets or automatic filename mapping, remote collision policy, optional verification, remote directory creation, shared transfer progress, and cancellation-aware async uploads for both FTP and FTPS sessions.</para>
/// <example>
///   <para>Upload a single file to the current remote working directory.</para>
///   <code>Send-FTPFile -Client $ftp -LocalPath '.\build\app.zip'</code>
/// </example>
/// <example>
///   <para>Upload a file to an explicit FTPS destination and overwrite an existing file.</para>
///   <code>Send-FTPFile -Client $ftp -LocalFile (Get-Item '.\release\site.tar.gz') -RemotePath '/incoming/site.tar.gz' -RemoteExists Overwrite -VerifyOptions Retry</code>
/// </example>
/// <example>
///   <para>Upload multiple files, create the remote folder if needed, and show progress.</para>
///   <code>Send-FTPFile -Client $ftp -LocalPath '.\logs\app.log','.\logs\web.log' -RemotePath '/archive/logs' -CreateRemoteDirectory -ShowProgress</code>
/// </example>
/// </summary>

[Alias(new string[] { "Add-FTPFile" })]
[Cmdlet("Send", "FTPFile")]
public sealed class CmdletSendFtpFile : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local File.
	/// </summary>

	[Parameter]
	public FileInfo[]? LocalFile { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter]
	public string[]? LocalPath { get; set; }
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
	/// Gets or sets the error Handling.
	/// </summary>

	[Parameter]
	public FtpError ErrorHandling { get; set; } = FtpError.None;
	/// <summary>
	/// Gets or sets the create Remote Directory.
	/// </summary>

	[Parameter]
	public SwitchParameter CreateRemoteDirectory { get; set; }
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
		if (Client == null) {
			return;
		}

		try {
			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = await TransferettoClient.UploadFtpFilesAsync(
				Client,
				RemotePath!,
				LocalPath!,
				LocalFile,
				RemoteExists,
				VerifyOptions,
				ErrorHandling,
				CreateRemoteDirectory.IsPresent,
				options,
				CancelToken).ConfigureAwait(false);
			WriteObject(sendToPipeline, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "SendFtpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
