using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Close-SFTPStream cmdlet.
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

