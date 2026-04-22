using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads POSIX-style permission bits for a remote FTP item.</para>
/// <para type="description">Returns the remote mode/permission information reported by the FTP server so scripts can inspect Unix-style access flags before applying changes.</para>
/// <example>
///   <para>Read permissions for a remote file.</para>
///   <code>Get-FTPChmod -Client $ftp -RemotePath '/wwwroot/appsettings.json'</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPChmod")]
public sealed class CmdletGetFtpChmod : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter]
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
			WriteObject(TransferettoClient.GetFtpChmod(Client, RemotePath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpChmodFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
