using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Sets the last modified time for a remote FTP item.</para>
/// <para type="description">Writes a remote timestamp and can optionally return the updated item metadata, which is helpful for preserving deployment timestamps after upload.</para>
/// <example>
///   <para>Set a remote file timestamp to a specific value.</para>
///   <code>Set-FTPModifiedTime -Client $ftp -RemotePath '/wwwroot/index.html' -ModifiedTime (Get-Date '2026-04-22T12:00:00Z')</code>
/// </example>
/// <example>
///   <para>Update the timestamp and return the refreshed remote item.</para>
///   <code>Set-FTPModifiedTime -Client $ftp -RemotePath '/wwwroot/index.html' -ModifiedTime (Get-Date) -PassThru</code>
/// </example>
/// </summary>

[Cmdlet("Set", "FTPModifiedTime")]
public sealed class CmdletSetFtpModifiedTime : PSCmdlet
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
	/// Gets or sets the modified Time.
	/// </summary>

	[Parameter(Mandatory = true)]
	public DateTime ModifiedTime { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.SetFtpModifiedTime(Client, RemotePath!, ModifiedTime);
			if (PassThru.IsPresent)
			{
				WriteObject(TransferettoClient.GetFtpItem(Client, RemotePath!));
			}
			else
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpModifiedTimeFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
