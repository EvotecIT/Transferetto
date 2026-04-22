using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPOption cmdlet.
/// </summary>

[Cmdlet("Set", "FTPOption")]
public sealed class CmdletSetFtpOption : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the retry Attempts.
	/// </summary>

	[Parameter]
	public int? RetryAttempts { get; set; }
	/// <summary>
	/// Gets or sets the download Zero Byte Files.
	/// </summary>

	[Parameter]
	public bool? DownloadZeroByteFiles { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			TransferettoClient.SetFtpOption(Client, RetryAttempts, DownloadZeroByteFiles);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpOptionFailed", ErrorCategory.InvalidArgument, Client.Host));
		}
	}
}

