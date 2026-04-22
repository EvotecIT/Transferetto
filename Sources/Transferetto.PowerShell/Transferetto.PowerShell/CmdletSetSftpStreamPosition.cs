using System;
using System.IO;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SFTPStreamPosition cmdlet.
/// </summary>

[Cmdlet("Set", "SFTPStreamPosition")]
public sealed class CmdletSetSftpStreamPosition : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpStreamSession? StreamSession { get; set; }
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
			WriteObject(TransferettoClient.SeekSftpStream(StreamSession, Offset, Origin));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpStreamPositionFailed", ErrorCategory.InvalidOperation, StreamSession.RemotePath));
		}
	}
}

