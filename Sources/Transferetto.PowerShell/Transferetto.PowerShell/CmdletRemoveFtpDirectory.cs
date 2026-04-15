using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Remove-FTPDirectory cmdlet.
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

