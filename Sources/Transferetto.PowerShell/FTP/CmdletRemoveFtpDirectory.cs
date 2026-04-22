using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Removes a directory from an FTP or FTPS server.</para>
/// <para type="description">Deletes a remote FTP directory and can pass explicit listing options for servers that need additional directory enumeration behavior during recursive removal.</para>
/// <example>
///   <para>Remove a remote FTP directory.</para>
///   <code>Remove-FTPDirectory -Client $ftp -RemotePath '/incoming/old-release'</code>
/// </example>
/// <example>
///   <para>Remove a directory while forcing a specific listing mode.</para>
///   <code>Remove-FTPDirectory -Client $ftp -RemotePath '/staging/tmp' -FtpListOption AllFiles</code>
/// </example>
/// </summary>

[Cmdlet("Remove", "FTPDirectory")]
public sealed class CmdletRemoveFtpDirectory : PSCmdlet
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
	/// Gets or sets the ftp List Option.
	/// </summary>

	[Parameter]
	public FtpListOption FtpListOption { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			bool flag = base.MyInvocation.BoundParameters.ContainsKey("FtpListOption");
			TransferettoClient.RemoveFtpDirectory(Client, RemotePath!, flag ? new FtpListOption?(FtpListOption) : ((FtpListOption?)null));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RemoveFtpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
