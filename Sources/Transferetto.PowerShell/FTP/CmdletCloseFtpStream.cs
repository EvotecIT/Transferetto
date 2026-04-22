using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Closes an open FTP stream session.</para>
/// <para type="description">Releases the low-level FTP stream created by <c>Open-FTPStream</c> so the remote file handle and associated transfer resources are closed cleanly.</para>
/// <example>
///   <para>Close the FTP stream after reading or writing is complete.</para>
///   <code>Close-FTPStream -StreamSession $stream</code>
/// </example>
/// </summary>

[Cmdlet("Close", "FTPStream")]
public sealed class CmdletCloseFtpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoFtpStreamSession? StreamSession { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.CloseFtpStream(StreamSession);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CloseFtpStreamFailed", ErrorCategory.CloseError, StreamSession.RemotePath));
		}
	}
}
