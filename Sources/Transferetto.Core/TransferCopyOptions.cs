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

    /// <summary>Gets or sets an expected SHA-256 hex digest checked before staged content is committed.</summary>
    public string? ExpectedSha256 { get; set; }

    /// <summary>Gets or sets whether to read the committed destination back and verify its SHA-256 digest.</summary>
    /// <remarks>A failed readback reports failure after the write; it does not roll back the committed destination.</remarks>
    public bool VerifyDestination { get; set; }
}
