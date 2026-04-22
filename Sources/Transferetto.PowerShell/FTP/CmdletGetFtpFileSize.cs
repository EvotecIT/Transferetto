using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Returns the size of a remote FTP file.</para>
/// <para type="description">Reads the remote file length and can fall back to a caller-supplied default value when the size cannot be determined reliably.</para>
/// <example>
///   <para>Return the size of a remote archive.</para>
///   <code>Get-FTPFileSize -Client $ftp -RemotePath '/incoming/site.zip'</code>
/// </example>
/// <example>
///   <para>Return zero when the server cannot report the size.</para>
///   <code>Get-FTPFileSize -Client $ftp -RemotePath '/incoming/site.zip' -DefaultValue 0</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPFileSize")]
public sealed class CmdletGetFtpFileSize : PSCmdlet
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
	/// Gets or sets the default Value.
	/// </summary>

	[Parameter]
	public long DefaultValue { get; set; } = -1L;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpFileSize(Client, RemotePath!, DefaultValue));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpFileSizeFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
