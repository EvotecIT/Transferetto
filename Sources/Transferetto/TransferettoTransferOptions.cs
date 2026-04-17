using System;
using System.Threading;

namespace Transferetto;
/// <summary>
/// Provides shared options for transfer operations.
/// </summary>

public sealed class TransferettoTransferOptions {
    /// <summary>
    /// Gets or sets a token used to cancel a running transfer.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    /// <summary>
    /// Gets or sets the progress sink used by transfer operations.
    /// </summary>

    public IProgress<TransferettoTransferProgress>? Progress { get; set; }
    /// <summary>
    /// Gets or sets the minimum number of bytes between progress reports.
    /// </summary>

    public long ProgressIntervalBytes { get; set; } = 65536;
}
