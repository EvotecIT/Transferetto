using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-FTPItem cmdlet.
/// </summary>

[Cmdlet("Get", "FTPItem")]
public sealed class CmdletGetFtpItem : PSCmdlet
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
	/// Gets or sets the follow Links.
	/// </summary>

	[Parameter]
	public SwitchParameter FollowLinks { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpItem(Client, RemotePath!, FollowLinks.IsPresent));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpItemFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

