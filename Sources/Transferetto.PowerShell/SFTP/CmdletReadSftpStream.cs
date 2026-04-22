using System;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads bytes or text from an open SFTP stream session.</para>
/// <para type="description">Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental SFTP stream reads can be automated efficiently.</para>
/// <example>
///   <para>Read the next chunk of bytes from the SFTP stream.</para>
///   <code>Read-SFTPStream -StreamSession $stream -Count 8192</code>
/// </example>
/// <example>
///   <para>Read text content from the SFTP stream.</para>
///   <code>Read-SFTPStream -StreamSession $stream -AsText -Count 4096</code>
/// </example>
/// </summary>

[Cmdlet("Read", "SFTPStream", DefaultParameterSetName = "Bytes")]
public sealed class CmdletReadSftpStream : AsyncPSCmdlet
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
			TransferettoSftpStreamReadResult transferettoSftpStreamReadResult = await TransferettoClient.ReadSftpStreamAsync(StreamSession, Count, options, CancelToken).ConfigureAwait(false);
			if (base.ParameterSetName == "Text")
			{
				WriteObject((Encoding ?? System.Text.Encoding.UTF8).GetString(transferettoSftpStreamReadResult.Data));
			}
			else
			{
				WriteObject(transferettoSftpStreamReadResult);
			}
		}
		catch (OperationCanceledException) when (CancelToken.IsCancellationRequested)
		{
			// StopProcessing requested cancellation.
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ReadSftpStreamFailed", ErrorCategory.ReadError, StreamSession.RemotePath));
		}
	}
}
