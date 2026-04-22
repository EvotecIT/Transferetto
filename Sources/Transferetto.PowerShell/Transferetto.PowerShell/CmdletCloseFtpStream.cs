using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Close-FTPStream cmdlet.
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

