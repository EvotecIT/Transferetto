using System;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads bytes or text from an open FTP stream session.</para>
/// <para type="description">Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental FTP stream reads can be scripted without buffering an entire file up front.</para>
/// <example>
///   <para>Read the next chunk of bytes from the stream.</para>
///   <code>Read-FTPStream -StreamSession $stream -Count 8192</code>
/// </example>
/// <example>
///   <para>Read text from the stream by decoding the returned bytes as UTF-8.</para>
///   <code>Read-FTPStream -StreamSession $stream -Count 4096 -AsText</code>
/// </example>
/// </summary>

[Cmdlet("Read", "FTPStream", DefaultParameterSetName = "Bytes")]
public sealed class CmdletReadFtpStream : AsyncPSCmdlet
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
			TransferettoTransferOptions options = new()
			{
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			TransferettoFtpStreamReadResult transferettoFtpStreamReadResult = await TransferettoClient.ReadFtpStreamAsync(StreamSession, Count, options, CancelToken).ConfigureAwait(false);
			if (base.ParameterSetName == "Text")
			{
				WriteObject((Encoding ?? System.Text.Encoding.UTF8).GetString(transferettoFtpStreamReadResult.Data));
			}
			else
			{
				WriteObject(transferettoFtpStreamReadResult);
			}
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadFtpStreamFailed", ErrorCategory.ReadError, StreamSession.RemotePath));
		}
	}
}
