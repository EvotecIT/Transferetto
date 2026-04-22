using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Write-SFTPStream cmdlet.
/// </summary>

[Cmdlet("Write", "SFTPStream", DefaultParameterSetName = "Text")]
public sealed class CmdletWriteSftpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpStreamSession? StreamSession { get; set; }
	/// <summary>
	/// Gets or sets the text.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Text")]
	public string? Text { get; set; }
	/// <summary>
	/// Gets or sets the encoding.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public Encoding? Encoding { get; set; }
	/// <summary>
	/// Gets or sets the byte Content.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Bytes")]
	public byte[]? ByteContent { get; set; }
	/// <summary>
	/// Gets or sets the flush.
	/// </summary>

	[Parameter]
	public SwitchParameter Flush { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			byte[] content = ((base.ParameterSetName == "Bytes") ? (ByteContent ?? Array.Empty<byte>()) : (Encoding ?? System.Text.Encoding.UTF8).GetBytes(Text ?? string.Empty));
			WriteObject(TransferettoClient.WriteSftpStream(StreamSession, content, Flush.IsPresent));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "WriteSftpStreamFailed", ErrorCategory.WriteError, StreamSession.RemotePath));
		}
	}
}

