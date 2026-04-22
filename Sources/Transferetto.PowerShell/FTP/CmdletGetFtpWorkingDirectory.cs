using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Returns the current working directory for an FTP or FTPS session.</para>
/// <para type="description">Exposes the session’s active remote working directory so scripts can confirm navigation state before relative listing, upload, download, or rename operations.</para>
/// <example>
///   <para>Show the current FTP working directory.</para>
///   <code>Get-FTPWorkingDirectory -Client $ftp</code>
/// </example>
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
