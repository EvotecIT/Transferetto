using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-FTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Get-FTPFile" })]
[Cmdlet("Receive", "FTPFile", DefaultParameterSetName = "Text")]
public sealed class CmdletReceiveFtpFile : PSCmdlet
{
	private readonly CancellationTokenSource cancellationTokenSource = new();
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
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath))
		{
			return;
		}
		try
		{
			string localPath = LocalPath!;
			List<string> list = ResolveRemotePaths().ToList();
			if (list.Count != 0)
			{
				bool flag = Directory.Exists(localPath);
				if (list.Count > 1 && File.Exists(localPath))
				{
					throw new PSArgumentException("LocalPath must reference a directory when downloading multiple remote files.", nameof(LocalPath));
				}
				TransferettoTransferOptions options = new()
				{
					CancellationToken = cancellationTokenSource.Token,
					ProgressIntervalBytes = ProgressIntervalBytes,
					Progress = ShowProgress.IsPresent ? new CmdletTransferProgress(this) : null
				};
				IReadOnlyList<TransferettoTransferResult> sendToPipeline = ((!(list.Count > 1 || flag)) ? new TransferettoTransferResult[1] { TransferettoClient.DownloadFtpFile(Client, localPath, list[0], LocalExists, VerifyOptions, options) } : TransferettoClient.DownloadFtpFiles(Client, localPath, list, LocalExists, VerifyOptions, FtpError, options));
				if (!Suppress.IsPresent)
				{
					WriteObject(sendToPipeline, enumerateCollection: true);
				}
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReceiveFtpFileFailed", ErrorCategory.ReadError, LocalPath));
		}
	}

	/// <inheritdoc/>
	protected override void StopProcessing()
	{
		cancellationTokenSource.Cancel();
		base.StopProcessing();
	}

	private IEnumerable<string> ResolveRemotePaths()
	{
		PSObject[]? remoteFile = RemoteFile;
		if (remoteFile != null && remoteFile.Length > 0)
		{
			PSObject[]? remoteFile2 = RemoteFile;
			foreach (PSObject remoteFile3 in remoteFile2!)
			{
				string? type = remoteFile3.Properties["Type"]?.Value?.ToString();
				string? fullName = remoteFile3.Properties["FullName"]?.Value?.ToString();
				if (!string.IsNullOrWhiteSpace(fullName))
				{
					if (!string.Equals(type, "File", StringComparison.OrdinalIgnoreCase) && !string.Equals(type, "0", StringComparison.OrdinalIgnoreCase))
					{
						WriteWarning($"Receive-FTPFile - Given path {fullName} is {type}. Skipping.");
					}
					else
					{
						yield return fullName!;
					}
				}
			}
		}
		string[]? remotePath = RemotePath;
		if (remotePath == null || remotePath.Length <= 0)
		{
			yield break;
		}
		foreach (string item in RemotePath!.Where((string path) => !string.IsNullOrWhiteSpace(path)))
		{
			yield return item;
		}
	}

	private sealed class CmdletTransferProgress : IProgress<TransferettoTransferProgress>
	{
		private readonly PSCmdlet cmdlet;

		public CmdletTransferProgress(PSCmdlet cmdlet)
		{
			this.cmdlet = cmdlet;
		}

		public void Report(TransferettoTransferProgress value)
		{
			int percentComplete = value.PercentComplete ?? -1;
			string activity = $"{value.Protocol} {value.Direction}";
			string status = value.TotalBytes.HasValue
				? $"{value.BytesTransferred} of {value.TotalBytes.Value} bytes"
				: $"{value.BytesTransferred} bytes";
			cmdlet.WriteProgress(new ProgressRecord(0, activity, status)
			{
				PercentComplete = percentComplete,
				CurrentOperation = value.RemotePath ?? value.LocalPath
			});
		}
	}
}
