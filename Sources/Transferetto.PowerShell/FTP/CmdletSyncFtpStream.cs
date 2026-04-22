using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Sync-FTPStream cmdlet.
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
