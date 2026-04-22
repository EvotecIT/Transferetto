using System;
using System.IO;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Moves the current position within an open FTP stream session.</para>
/// <para type="description">Seeks to a new offset in the FTP stream so callers can reread, skip ahead, or resume low-level stream-based operations from a specific location.</para>
/// <example>
///   <para>Seek to the beginning of the stream.</para>
///   <code>Set-FTPStreamPosition -StreamSession $stream -Offset 0 -Origin Begin</code>
/// </example>
/// <example>
///   <para>Skip forward 128 bytes from the current position.</para>
///   <code>Set-FTPStreamPosition -StreamSession $stream -Offset 128 -Origin Current</code>
/// </example>
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
