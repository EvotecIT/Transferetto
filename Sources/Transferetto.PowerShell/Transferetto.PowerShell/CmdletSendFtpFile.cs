using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-FTPFile cmdlet.
/// </summary>

[Alias(new string[] { "Add-FTPFile" })]
[Cmdlet("Send", "FTPFile")]
public sealed class CmdletSendFtpFile : PSCmdlet
{
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			IReadOnlyList<TransferettoTransferResult> sendToPipeline = TransferettoClient.UploadFtpFiles(Client, RemotePath!, LocalPath!, LocalFile, RemoteExists, VerifyOptions, ErrorHandling, CreateRemoteDirectory.IsPresent);
			WriteObject(sendToPipeline, enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendFtpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

