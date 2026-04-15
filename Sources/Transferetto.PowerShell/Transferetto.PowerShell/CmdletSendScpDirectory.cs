using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SCPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Add-SCPDirectory" })]
[Cmdlet("Send", "SCPDirectory")]
public sealed class CmdletSendScpDirectory : PSCmdlet
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
			WriteObject(TransferettoClient.UploadScpDirectory(ScpClient, LocalPath!, RemotePath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendScpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

