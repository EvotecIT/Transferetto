using System;
using System.Management.Automation;
using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-FTPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Add-FTPDirectory" })]
[Cmdlet("Send", "FTPDirectory")]
public sealed class CmdletSendFtpDirectory : PSCmdlet
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.UploadFtpDirectory(Client, LocalPath!, RemotePath!, FolderSyncMode, RemoteExists, VerifyOptions, Rules), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendFtpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

