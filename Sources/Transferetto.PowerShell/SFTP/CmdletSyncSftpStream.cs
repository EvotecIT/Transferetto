using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Flushes buffered writes for an open SFTP stream session.</para>
/// <para type="description">Forces pending SFTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.</para>
/// <example>
///   <para>Flush pending writes to the remote SFTP file.</para>
///   <code>Sync-SFTPStream -StreamSession $stream</code>
/// </example>
/// </summary>

[Cmdlet("Sync", "SFTPStream")]
public sealed class CmdletSyncSftpStream : PSCmdlet
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
			WriteObject(TransferettoClient.FlushSftpStream(StreamSession));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SyncSftpStreamFailed", ErrorCategory.WriteError, StreamSession.RemotePath));
		}
	}
}
