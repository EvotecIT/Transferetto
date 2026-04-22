using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-SFTPWorkingDirectory cmdlet.
/// </summary>

[Cmdlet("Get", "SFTPWorkingDirectory")]
public sealed class CmdletGetSftpWorkingDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSftpSession? SftpClient { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null)
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetSftpWorkingDirectory(SftpClient));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpWorkingDirectoryFailed", ErrorCategory.ReadError, SftpClient.Host));
		}
	}
}

