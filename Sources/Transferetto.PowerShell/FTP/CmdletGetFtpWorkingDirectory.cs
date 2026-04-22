using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-FTPWorkingDirectory cmdlet.
/// </summary>

[Cmdlet("Get", "FTPWorkingDirectory")]
public sealed class CmdletGetFtpWorkingDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoFtpSession? Client { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpWorkingDirectory(Client));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpWorkingDirectoryFailed", ErrorCategory.ReadError, Client.Host));
		}
	}
}

