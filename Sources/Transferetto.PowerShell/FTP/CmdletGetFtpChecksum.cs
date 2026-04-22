using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Calculates or retrieves a checksum for a remote FTP file.</para>
/// <para type="description">Requests a remote hash from the FTP server by using the selected algorithm, which is useful for post-upload verification and drift detection workflows.</para>
/// <example>
///   <para>Request an MD5 checksum for a remote file.</para>
///   <code>Get-FTPChecksum -Client $ftp -RemotePath '/incoming/site.zip'</code>
/// </example>
/// <example>
///   <para>Request a SHA-256 checksum when the server supports it.</para>
///   <code>Get-FTPChecksum -Client $ftp -RemotePath '/incoming/site.zip' -HashAlgorithm SHA256</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPChecksum")]
public sealed class CmdletGetFtpChecksum : PSCmdlet
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
	/// Gets or sets the hash Algorithm.
	/// </summary>

	[Parameter]
	public FtpHashAlgorithm HashAlgorithm { get; set; } = FtpHashAlgorithm.MD5;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpChecksum(Client, RemotePath!, HashAlgorithm));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpChecksumFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
