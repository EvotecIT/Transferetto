using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SCPFile cmdlet.
/// </summary>

[Alias(new string[] { "Add-SCPFile" })]
[Cmdlet("Send", "SCPFile")]
public sealed class CmdletSendScpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoScpSession? ScpClient { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ScpClient == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.UploadScpFile(ScpClient, LocalPath!, RemotePath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendScpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

