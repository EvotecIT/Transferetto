using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Flushes buffered writes for an open FTP stream session.</para>
/// <para type="description">Forces pending FTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.</para>
/// <example>
///   <para>Flush pending writes to the remote FTP file.</para>
///   <code>Sync-FTPStream -StreamSession $stream</code>
/// </example>
/// </summary>

[Cmdlet("Sync", "FTPStream")]
public sealed class CmdletSyncFtpStream : PSCmdlet
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
			WriteObject(TransferettoClient.FlushFtpStream(StreamSession));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SyncFtpStreamFailed", ErrorCategory.WriteError, StreamSession.RemotePath));
		}
	}
}
