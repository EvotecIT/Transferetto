using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Opens a readable or writable SFTP stream for a remote file.</para>
/// <para type="description">Creates a reusable stream session for low-level SFTP access when callers need incremental reads or writes instead of a full-file transfer cmdlet.</para>
/// <example>
///   <para>Open a remote file for reading through SFTP.</para>
///   <code>$stream = Open-SFTPStream -SftpClient $sftp -Path '/srv/app/settings.json'</code>
/// </example>
/// <example>
///   <para>Open a remote file for writing through SFTP.</para>
///   <code>$stream = Open-SFTPStream -SftpClient $sftp -Path '/srv/app/settings.json' -Mode Write</code>
/// </example>
/// </summary>

[Cmdlet("Open", "SFTPStream")]
public sealed class CmdletOpenSftpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the mode.
	/// </summary>

	[Parameter]
	public TransferettoSftpStreamMode Mode { get; set; } = TransferettoSftpStreamMode.Read;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.OpenSftpStream(SftpClient, Path!, Mode));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "OpenSftpStreamFailed", ErrorCategory.OpenError, Path));
		}
	}
}
