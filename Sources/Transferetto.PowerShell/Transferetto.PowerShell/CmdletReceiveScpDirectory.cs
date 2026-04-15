using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Receive-SCPDirectory cmdlet.
/// </summary>

[Alias(new string[] { "Get-SCPDirectory" })]
[Cmdlet("Receive", "SCPDirectory")]
public sealed class CmdletReceiveScpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoScpSession? ScpClient { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ScpClient == null || string.IsNullOrWhiteSpace(RemotePath) || string.IsNullOrWhiteSpace(LocalPath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.DownloadScpDirectory(ScpClient, RemotePath!, LocalPath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReceiveScpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

