using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-FTPFileSize cmdlet.
/// </summary>

[Cmdlet("Get", "FTPFileSize")]
public sealed class CmdletGetFtpFileSize : PSCmdlet
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
	/// Gets or sets the default Value.
	/// </summary>

	[Parameter]
	public long DefaultValue { get; set; } = -1L;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpFileSize(Client, RemotePath!, DefaultValue));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpFileSizeFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

