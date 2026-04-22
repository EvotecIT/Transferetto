using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Read-SFTPStream cmdlet.
/// </summary>

[Cmdlet("Read", "SFTPStream", DefaultParameterSetName = "Bytes")]
public sealed class CmdletReadSftpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSftpStreamSession? StreamSession { get; set; }
	/// <summary>
	/// Gets or sets the count.
	/// </summary>

	[Parameter]
	public int Count { get; set; } = 4096;
	/// <summary>
	/// Gets or sets the as Text.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public SwitchParameter AsText { get; set; }
	/// <summary>
	/// Gets or sets the encoding.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public Encoding? Encoding { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			TransferettoSftpStreamReadResult transferettoSftpStreamReadResult = TransferettoClient.ReadSftpStream(StreamSession, Count);
			if (base.ParameterSetName == "Text")
			{
				WriteObject((Encoding ?? System.Text.Encoding.UTF8).GetString(transferettoSftpStreamReadResult.Data));
			}
			else
			{
				WriteObject(transferettoSftpStreamReadResult);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadSftpStreamFailed", ErrorCategory.ReadError, StreamSession.RemotePath));
		}
	}
}

