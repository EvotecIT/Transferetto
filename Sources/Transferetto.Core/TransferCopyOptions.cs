using System;

namespace Transferetto.Core;

/// <summary>
/// Controls a provider-neutral endpoint-to-endpoint transfer.
/// </summary>
public sealed class TransferCopyOptions {
    /// <summary>Gets or sets destination write behavior.</summary>
    public TransferWriteOptions WriteOptions { get; set; } = new();

    /// <summary>Gets or sets an optional progress sink.</summary>
    public IProgress<TransferProgress>? Progress { get; set; }

    /// <summary>Gets or sets the minimum number of bytes between progress reports.</summary>
    public long ProgressIntervalBytes { get; set; } = 65536;
}
