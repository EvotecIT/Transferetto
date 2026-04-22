using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Sync-SFTPStream cmdlet.
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
