using System;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Write-FTPStream cmdlet.
/// </summary>

[Cmdlet("Write", "FTPStream", DefaultParameterSetName = "Text")]
public sealed class CmdletWriteFtpStream : AsyncPSCmdlet
{
	/// <summary>
	/// Gets or sets the stream Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpStreamSession? StreamSession { get; set; }
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

	/// <summary>
	/// Gets or sets a value indicating whether stream progress is displayed.
	/// </summary>
	[Parameter]
	public SwitchParameter ShowProgress { get; set; }

	/// <summary>
	/// Gets or sets the minimum number of bytes between progress updates.
	/// </summary>
	[Parameter]
	public long ProgressIntervalBytes { get; set; } = 65536;

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync()
	{
		if (StreamSession == null)
		{
			return;
		}
		try
		{
			byte[] content = ((base.ParameterSetName == "Bytes") ? (ByteContent ?? Array.Empty<byte>()) : (Encoding ?? System.Text.Encoding.UTF8).GetBytes(Text ?? string.Empty));
			TransferettoTransferOptions options = new()
			{
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			WriteObject(await TransferettoClient.WriteFtpStreamAsync(StreamSession, content, Flush.IsPresent, options, CancelToken).ConfigureAwait(false));
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "WriteFtpStreamFailed", ErrorCategory.WriteError, StreamSession.RemotePath));
		}
	}
}
