using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-FTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Get-FTPFile" })]
[Cmdlet("Receive", "FTPFile", DefaultParameterSetName = "Text")]
public sealed class CmdletReceiveFtpFile : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(ParameterSetName = "Text", Mandatory = true)]
	[Parameter(ParameterSetName = "Native", Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote File.
	/// </summary>

	[Parameter(ParameterSetName = "Native")]
	public PSObject[]? RemoteFile { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public string[]? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the local Exists.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public FtpLocalExists LocalExists { get; set; } = FtpLocalExists.Skip;
	/// <summary>
	/// Gets or sets the verify Options.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public FtpVerify VerifyOptions { get; set; } = FtpVerify.None;
	/// <summary>
	/// Gets or sets the ftp Error.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public FtpError FtpError { get; set; } = FtpError.Stop;
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public SwitchParameter Suppress { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether transfer progress is displayed.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public SwitchParameter ShowProgress { get; set; }
	/// <summary>
	/// Gets or sets the minimum number of bytes between progress updates.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	[Parameter(ParameterSetName = "Native")]
	public long ProgressIntervalBytes { get; set; } = 65536;

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync() {
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath)) {
			return;
		}

		try {
			string localPath = LocalPath!;
			List<string> remotePaths = ResolveRemotePaths().ToList();
			if (remotePaths.Count == 0) {
				return;
			}

			bool localPathIsDirectory = Directory.Exists(localPath);
			if (remotePaths.Count > 1 && File.Exists(localPath)) {
				throw new PSArgumentException("LocalPath must reference a directory when downloading multiple remote files.", nameof(LocalPath));
			}

			TransferettoTransferOptions options = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = !(remotePaths.Count > 1 || localPathIsDirectory)
				? new[] {
					await TransferettoClient.DownloadFtpFileAsync(
						Client,
						localPath,
						remotePaths[0],
						LocalExists,
						VerifyOptions,
						options,
						CancelToken).ConfigureAwait(false)
				}
				: await TransferettoClient.DownloadFtpFilesAsync(
					Client,
					localPath,
					remotePaths,
					LocalExists,
					VerifyOptions,
					FtpError,
					options,
					CancelToken).ConfigureAwait(false);
			if (!Suppress.IsPresent) {
				WriteObject(sendToPipeline, enumerateCollection: true);
			}
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "ReceiveFtpFileFailed", ErrorCategory.ReadError, LocalPath));
		}
	}

	private IEnumerable<string> ResolveRemotePaths() {
		PSObject[]? remoteFile = RemoteFile;
		if (remoteFile != null && remoteFile.Length > 0) {
			foreach (PSObject remoteFileItem in remoteFile) {
				string? type = remoteFileItem.Properties["Type"]?.Value?.ToString();
				if (remoteFileItem.Properties["FullName"]?.Value?.ToString() is not string fullNameValue || string.IsNullOrWhiteSpace(fullNameValue)) {
					continue;
				}

				if (!string.Equals(type, "File", StringComparison.OrdinalIgnoreCase) && !string.Equals(type, "0", StringComparison.OrdinalIgnoreCase)) {
					WriteWarning($"Receive-FTPFile - Given path {fullNameValue} is {type}. Skipping.");
				} else {
					yield return fullNameValue;
				}
			}
		}

		string[]? remotePath = RemotePath;
		if (remotePath == null || remotePath.Length <= 0) {
			yield break;
		}

		foreach (string? item in remotePath) {
			if (!string.IsNullOrWhiteSpace(item)) {
				yield return item;
			}
		}
	}
}
