using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Returns the last modified time for a remote FTP item.</para>
/// <para type="description">Reads the remote timestamp reported by the FTP server, which is useful for deployment comparisons, freshness checks, and timestamp synchronization.</para>
/// <example>
///   <para>Check when a remote file was last modified.</para>
///   <code>Get-FTPModifiedTime -Client $ftp -RemotePath '/wwwroot/index.html'</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPModifiedTime")]
public sealed class CmdletGetFtpModifiedTime : PSCmdlet
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpModifiedTime(Client, RemotePath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpModifiedTimeFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
