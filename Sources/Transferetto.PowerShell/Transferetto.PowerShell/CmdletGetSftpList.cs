using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-SFTPList cmdlet.
/// </summary>

[Cmdlet("Get", "SFTPList")]
public sealed class CmdletGetSftpList : PSCmdlet
{
	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	[Alias(new string[] { "FtpPath" })]
	[Parameter]
	public string? Path { get; set; }
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
			WriteObject(TransferettoClient.GetSftpListing(SftpClient, Path!), enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpListFailed", ErrorCategory.ReadError, Path ?? SftpClient.Host));
		}
	}
}

