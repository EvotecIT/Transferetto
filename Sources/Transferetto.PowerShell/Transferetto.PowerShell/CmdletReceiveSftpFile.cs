using System;
using System.IO;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-SFTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Get-SFTPFile" })]
[Cmdlet("Receive", "SFTPFile")]
public sealed class CmdletReceiveSftpFile : PSCmdlet
{
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.DownloadSftpFile(SftpClient, RemotePath!, LocalPath!));
		}
		catch (Exception exception)
		{
			if (File.Exists(LocalPath))
			{
				File.Delete(LocalPath);
			}
			WriteError(new ErrorRecord(exception, "ReceiveSftpFileFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

