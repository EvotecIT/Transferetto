using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Synchronizes a local directory with an FTP or FTPS directory.</para>
/// <para type="description">Uses the shared Transferetto synchronization planner to upload or download missing and changed files, optionally mirror-delete extra destination items, filter paths by wildcard patterns, preserve timestamps, and preview planned work with dry-run mode.</para>
/// <example>
///   <para>Upload local changes to a remote FTP directory without deleting extra remote files.</para>
///   <code>Sync-FTPDirectory -Client $ftp -LocalPath '.\Website' -RemotePath '/wwwroot'</code>
/// </example>
/// <example>
///   <para>Preview a remote mirror operation before deleting extra remote files.</para>
///   <code>Sync-FTPDirectory -Client $ftp -LocalPath '.\Website' -RemotePath '/wwwroot' -Mode Mirror -DryRun</code>
/// </example>
/// <example>
///   <para>Download only log files from an FTP directory tree.</para>
///   <code>Sync-FTPDirectory -Client $ftp -Direction Download -RemotePath '/exports' -LocalPath '.\Exports' -Include '*.log'</code>
/// </example>
/// </summary>
[Cmdlet("Sync", "FTPDirectory")]
public sealed class CmdletSyncFtpDirectory : AsyncPSCmdlet {
	/// <summary>
	/// Gets or sets the FTP or FTPS session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }

	/// <summary>
	/// Gets or sets the local directory path.
	/// </summary>
	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }

	/// <summary>
	/// Gets or sets the remote FTP or FTPS directory path.
	/// </summary>
	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }

	/// <summary>
	/// Gets or sets the synchronization direction.
	/// </summary>
	[Parameter]
	public TransferettoSyncDirection Direction { get; set; } = TransferettoSyncDirection.Upload;

	/// <summary>
	/// Gets or sets whether synchronization updates destination items or mirrors deletes too.
	/// </summary>
	[Parameter]
	public TransferettoSyncMode Mode { get; set; } = TransferettoSyncMode.Update;

	/// <summary>
	/// Gets or sets how existing files are compared.
	/// </summary>
	[Parameter]
	public TransferettoSyncComparison Comparison { get; set; } = TransferettoSyncComparison.SizeOrLastWriteTime;

	/// <summary>
	/// Gets or sets wildcard include patterns matched against relative paths and names.
	/// </summary>
	[Parameter]
	public string[]? Include { get; set; }

	/// <summary>
	/// Gets or sets wildcard exclude patterns matched against relative paths and names.
	/// </summary>
	[Parameter]
	public string[]? Exclude { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether planned operations are returned without changing files.
	/// </summary>
	[Parameter]
	public SwitchParameter DryRun { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether changed existing files should not be overwritten.
	/// </summary>
	[Parameter]
	public SwitchParameter NoOverwrite { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether missing destination directories should not be created.
	/// </summary>
	[Parameter]
	public SwitchParameter NoCreateDirectories { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether timestamps should not be preserved after transfers.
	/// </summary>
	[Parameter]
	public SwitchParameter NoPreserveTimestamps { get; set; }

	/// <summary>
	/// Gets or sets the timestamp tolerance in seconds for timestamp comparisons.
	/// </summary>
	[Parameter]
	public int TimestampToleranceSeconds { get; set; } = 2;

	/// <summary>
	/// Gets or sets a value indicating whether transfer progress is displayed.
	/// </summary>
	[Parameter]
	public SwitchParameter ShowProgress { get; set; }

	/// <summary>
	/// Gets or sets the minimum number of bytes between progress updates.
	/// </summary>
	[Parameter]
	public long ProgressIntervalBytes { get; set; } = 65536;

	/// <inheritdoc/>
	protected override async Task ProcessRecordAsync() {
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath)) {
			return;
		}

		try {
			TransferettoSyncOptions syncOptions = CreateSyncOptions();
			TransferettoTransferOptions transferOptions = new() {
				CancellationToken = CancelToken,
				ProgressIntervalBytes = ProgressIntervalBytes,
				Progress = ShowProgress.IsPresent ? new TransferettoCmdletTransferProgress(this) : null
			};
			IReadOnlyList<TransferettoSyncResult> result = await TransferettoClient.SyncFtpDirectoryAsync(
				Client,
				LocalPath!,
				RemotePath!,
				syncOptions,
				transferOptions,
				CancelToken).ConfigureAwait(false);
			WriteObject(result, enumerateCollection: true);
		} catch (OperationCanceledException) when (CancelToken.IsCancellationRequested) {
			// StopProcessing requested cancellation.
		} catch (Exception exception) {
			WriteError(new ErrorRecord(exception, "SyncFtpDirectoryFailed", ErrorCategory.InvalidOperation, RemotePath));
		}
	}

	private TransferettoSyncOptions CreateSyncOptions() {
		return new TransferettoSyncOptions {
			Direction = Direction,
			Mode = Mode,
			Comparison = Comparison,
			IncludePatterns = Include,
			ExcludePatterns = Exclude,
			DryRun = DryRun.IsPresent,
			OverwriteExisting = !NoOverwrite.IsPresent,
			CreateDestinationDirectories = !NoCreateDirectories.IsPresent,
			PreserveTimestamps = !NoPreserveTimestamps.IsPresent,
			TimestampTolerance = TimeSpan.FromSeconds(TimestampToleranceSeconds >= 0 ? TimestampToleranceSeconds : 2)
		};
	}
}
