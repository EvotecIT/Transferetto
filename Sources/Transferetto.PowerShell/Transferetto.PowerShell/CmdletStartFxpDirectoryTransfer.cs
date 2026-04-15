using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Start-FXPDirectoryTransfer cmdlet.
/// </summary>

[Alias(new string[] { "Start-FXPDirectory" })]
[Cmdlet("Start", "FXPDirectoryTransfer")]
public sealed class CmdletStartFxpDirectoryTransfer : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Alias(new string[] { "SourceClient" })]
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the source Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? SourcePath { get; set; }
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>

	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? DestinationClient { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }
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
	public FtpVerify[]? VerifyOptions { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || DestinationClient == null || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.StartFxpDirectoryTransfer(Client, SourcePath!, DestinationClient, DestinationPath!, FolderSyncMode, RemoteExists, VerifyOptions.CombineFlags()), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "StartFxpDirectoryTransferFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}

