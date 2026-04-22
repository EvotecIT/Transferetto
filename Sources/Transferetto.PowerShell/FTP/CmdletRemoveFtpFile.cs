using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Removes a file from an FTP or FTPS server.</para>
/// <para type="description">Deletes a single remote FTP file, which fits cleanup, rollback, and artifact rotation workflows.</para>
/// <example>
///   <para>Delete a remote file.</para>
///   <code>Remove-FTPFile -Client $ftp -RemotePath '/incoming/old-site.zip'</code>
/// </example>
/// </summary>

[Cmdlet("Remove", "FTPFile")]
public sealed class CmdletRemoveFtpFile : PSCmdlet
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
			TransferettoClient.RemoveFtpFile(Client, RemotePath!);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RemoveFtpFileFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
