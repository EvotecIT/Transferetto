using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Opens a readable or writable FTP stream for a remote file.</para>
/// <para type="description">Creates a reusable stream session for low-level FTP file access when callers need incremental reads or writes instead of a full-file transfer cmdlet.</para>
/// <example>
///   <para>Open a remote file for reading.</para>
///   <code>$stream = Open-FTPStream -Client $ftp -RemotePath '/pub/example/readme.txt'</code>
/// </example>
/// <example>
///   <para>Open a remote file for writing.</para>
///   <code>$stream = Open-FTPStream -Client $ftp -RemotePath '/incoming/upload.txt' -Mode Write</code>
/// </example>
/// </summary>

[Cmdlet("Open", "FTPStream")]
public sealed class CmdletOpenFtpStream : PSCmdlet
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
	/// Gets or sets the mode.
	/// </summary>

	[Parameter]
	public TransferettoFtpStreamMode Mode { get; set; } = TransferettoFtpStreamMode.Read;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.OpenFtpStream(Client, RemotePath!, Mode));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "OpenFtpStreamFailed", ErrorCategory.OpenError, RemotePath));
		}
	}
}
