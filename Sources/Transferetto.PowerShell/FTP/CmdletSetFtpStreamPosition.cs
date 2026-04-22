using System;
using System.IO;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPStreamPosition cmdlet.
/// </summary>

[Cmdlet("Set", "FTPStreamPosition")]
public sealed class CmdletSetFtpStreamPosition : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpStreamSession? StreamSession { get; set; }
	/// <summary>
	/// Gets or sets the offset.
	/// </summary>

	[Parameter(Mandatory = true)]
	public long Offset { get; set; }
	/// <summary>
	/// Gets or sets the origin.
	/// </summary>

	[Parameter]
	public SeekOrigin Origin { get; set; } = SeekOrigin.Begin;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.SeekFtpStream(StreamSession, Offset, Origin));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpStreamPositionFailed", ErrorCategory.InvalidOperation, StreamSession.RemotePath));
		}
	}
}

