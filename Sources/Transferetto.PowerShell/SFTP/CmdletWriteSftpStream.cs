using System;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Writes text or bytes to an open SFTP stream session.</para>
/// <para type="description">Supports text encoding or raw byte writes, optional flush behavior, and progress-aware async execution for low-level SFTP upload or remote content-editing scenarios.</para>
/// <example>
///   <para>Write text to a remote SFTP stream.</para>
///   <code>Write-SFTPStream -StreamSession $stream -Text 'deployment ready' -Flush</code>
/// </example>
/// <example>
///   <para>Write raw bytes to the SFTP stream.</para>
///   <code>Write-SFTPStream -StreamSession $stream -ByteContent ([byte[]](1,2,3,4))</code>
/// </example>
/// </summary>

[Cmdlet("Write", "SFTPStream", DefaultParameterSetName = "Text")]
public sealed class CmdletWriteSftpStream : AsyncPSCmdlet
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
			WriteObject(await TransferettoClient.WriteSftpStreamAsync(StreamSession, content, Flush.IsPresent, options, CancelToken).ConfigureAwait(false));
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "WriteSftpStreamFailed", ErrorCategory.WriteError, StreamSession.RemotePath));
		}
	}
}
