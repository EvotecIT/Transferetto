using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Adjusts runtime options on an existing FTP session.</para>
/// <para type="description">Lets scripts fine-tune retry behavior and zero-byte download handling on a live session without reconnecting.</para>
/// <example>
///   <para>Increase retry attempts for a flaky connection.</para>
///   <code>Set-FTPOption -Client $ftp -RetryAttempts 5</code>
/// </example>
/// <example>
///   <para>Allow zero-byte files to be downloaded.</para>
///   <code>Set-FTPOption -Client $ftp -DownloadZeroByteFiles $true</code>
/// </example>
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
