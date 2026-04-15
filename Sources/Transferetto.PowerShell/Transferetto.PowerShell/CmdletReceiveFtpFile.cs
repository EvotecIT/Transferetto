using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-FTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Get-FTPFile" })]
[Cmdlet("Receive", "FTPFile", DefaultParameterSetName = "Text")]
public sealed class CmdletReceiveFtpFile : PSCmdlet
{
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
				IReadOnlyList<TransferettoTransferResult> sendToPipeline = ((!(list.Count > 1 || flag)) ? new TransferettoTransferResult[1] { TransferettoClient.DownloadFtpFile(Client, localPath, list[0], LocalExists, VerifyOptions) } : TransferettoClient.DownloadFtpFiles(Client, localPath, list, LocalExists, VerifyOptions, FtpError));
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
}
