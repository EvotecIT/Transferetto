using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPModifiedTime cmdlet.
/// </summary>

[Cmdlet("Set", "FTPModifiedTime")]
public sealed class CmdletSetFtpModifiedTime : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the modified Time.
	/// </summary>

	[Parameter(Mandatory = true)]
	public DateTime ModifiedTime { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.SetFtpModifiedTime(Client, RemotePath!, ModifiedTime);
			if (PassThru.IsPresent)
			{
				WriteObject(TransferettoClient.GetFtpItem(Client, RemotePath!));
			}
			else
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpModifiedTimeFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

