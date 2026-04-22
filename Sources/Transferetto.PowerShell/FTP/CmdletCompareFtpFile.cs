using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Compares a local file with a remote FTP file.</para>
/// <para type="description">Uses FluentFTP comparison strategies to determine whether a local file matches a remote file by size, hash, or server-supported auto-detection logic.</para>
/// <example>
///   <para>Compare a local artifact with the remote copy.</para>
///   <code>Compare-FTPFile -Client $ftp -LocalPath '.\publish\site.zip' -RemotePath '/incoming/site.zip'</code>
/// </example>
/// <example>
///   <para>Force checksum-based comparison when supported.</para>
///   <code>Compare-FTPFile -Client $ftp -LocalPath '.\publish\site.zip' -RemotePath '/incoming/site.zip' -CompareOption Checksum</code>
/// </example>
/// </summary>

[Cmdlet("Compare", "FTPFile")]
public sealed class CmdletCompareFtpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the compare Option.
	/// </summary>

	[Parameter]
	public FtpCompareOption CompareOption { get; set; } = FtpCompareOption.Auto;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.CompareFtpFile(Client, LocalPath!, RemotePath!, CompareOption));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CompareFtpFileFailed", ErrorCategory.InvalidData, RemotePath));
		}
	}
}
