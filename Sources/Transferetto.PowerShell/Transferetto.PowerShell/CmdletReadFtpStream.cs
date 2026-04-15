using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Read-FTPStream cmdlet.
/// </summary>

[Cmdlet("Read", "FTPStream", DefaultParameterSetName = "Bytes")]
public sealed class CmdletReadFtpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoFtpStreamSession? StreamSession { get; set; }
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
			TransferettoFtpStreamReadResult transferettoFtpStreamReadResult = TransferettoClient.ReadFtpStream(StreamSession, Count);
			if (base.ParameterSetName == "Text")
			{
				WriteObject((Encoding ?? System.Text.Encoding.UTF8).GetString(transferettoFtpStreamReadResult.Data));
			}
			else
			{
				WriteObject(transferettoFtpStreamReadResult);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadFtpStreamFailed", ErrorCategory.ReadError, StreamSession.RemotePath));
		}
	}
}

