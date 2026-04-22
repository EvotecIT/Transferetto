using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Closes an open SFTP stream session.</para>
/// <para type="description">Releases the low-level SFTP stream created by <c>Open-SFTPStream</c> so the remote file handle and associated transport resources are closed cleanly.</para>
/// <example>
///   <para>Close the SFTP stream after reading or writing is complete.</para>
///   <code>Close-SFTPStream -StreamSession $stream</code>
/// </example>
/// </summary>

[Cmdlet("Close", "SFTPStream")]
public sealed class CmdletCloseSftpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSftpStreamSession? StreamSession { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.CloseSftpStream(StreamSession);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CloseSftpStreamFailed", ErrorCategory.CloseError, StreamSession.RemotePath));
		}
	}
}
