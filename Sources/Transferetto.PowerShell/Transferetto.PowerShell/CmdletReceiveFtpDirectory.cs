using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-FTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Get-FTPDirectory" })]
[Cmdlet("Receive", "FTPDirectory")]
public sealed class CmdletReceiveFtpDirectory : PSCmdlet
{
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.DownloadFtpDirectory(Client, LocalPath!, RemotePath!, FolderSyncMode), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReceiveFtpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

