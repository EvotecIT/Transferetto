using System;
using System.Collections.Generic;

namespace Transferetto.Core;

/// <summary>Identifies one copy within a bounded transfer batch.</summary>
public sealed class TransferBatchItem {
    /// <summary>Initializes a copy request.</summary>
    public TransferBatchItem(ITransferEndpoint source, string sourcePath, ITransferEndpoint destination, string destinationPath) {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        SourcePath = !string.IsNullOrEmpty(sourcePath) ? sourcePath : throw new ArgumentException("A source path is required.", nameof(sourcePath));
        DestinationPath = !string.IsNullOrEmpty(destinationPath) ? destinationPath : throw new ArgumentException("A destination path is required.", nameof(destinationPath));
    }

    /// <summary>Gets the source endpoint.</summary>
    public ITransferEndpoint Source { get; }
    /// <summary>Gets the source item path.</summary>
    public string SourcePath { get; }
    /// <summary>Gets the destination endpoint.</summary>
    public ITransferEndpoint Destination { get; }
    /// <summary>Gets the destination item path.</summary>
    public string DestinationPath { get; }
    /// <summary>Gets or sets options specific to this item.</summary>
    public TransferCopyOptions? Options { get; set; }
}

/// <summary>Controls bounded batch scheduling and failure behavior.</summary>
public sealed class TransferBatchOptions {
    /// <summary>Gets or sets the maximum number of active copies. Defaults to four.</summary>
    public int MaxConcurrency { get; set; } = 4;
    /// <summary>Gets or sets whether the first item failure stops scheduling new work and cancels in-flight items.</summary>
    public bool FailFast { get; set; }
    /// <summary>Gets or sets an optional aggregate progress observer.</summary>
    public IProgress<TransferBatchProgress>? Progress { get; set; }
}

/// <summary>Describes aggregate progress across a transfer batch.</summary>
public sealed class TransferBatchProgress {
    /// <summary>Gets the number of requested items.</summary>
    public int TotalItems { get; init; }
    /// <summary>Gets the number of items that have settled.</summary>
    public int CompletedItems { get; init; }
    /// <summary>Gets the unique source bytes consumed so far.</summary>
    public long BytesTransferred { get; init; }
}

/// <summary>Describes how one batch item settled.</summary>
public enum TransferBatchItemOutcome {
    /// <summary>The item was copied.</summary>
    Copied,
    /// <summary>The destination policy skipped the item.</summary>
    Skipped,
    /// <summary>The item failed.</summary>
    Failed,
    /// <summary>The item was canceled before completion.</summary>
    Canceled,
    /// <summary>The item was not started after fail-fast or cancellation.</summary>
    NotStarted
}

/// <summary>Records one item result without throwing away successful sibling results.</summary>
public sealed class TransferBatchItemResult {
    /// <summary>Gets the input index.</summary>
    public int Index { get; init; }
    /// <summary>Gets the original request.</summary>
    public TransferBatchItem Item { get; init; } = null!;
    /// <summary>Gets the settlement outcome.</summary>
    public TransferBatchItemOutcome Outcome { get; init; }
    /// <summary>Gets the successful receipt, when present.</summary>
    public TransferReceipt? Receipt { get; init; }
    /// <summary>Gets the failure or cancellation, when present.</summary>
    public Exception? Error { get; init; }
}

/// <summary>Records batch results in input order.</summary>
public sealed class TransferBatchResult {
    /// <summary>Gets all item results in input order.</summary>
    public IReadOnlyList<TransferBatchItemResult> Items { get; init; } = Array.Empty<TransferBatchItemResult>();
    /// <summary>Gets whether every requested copy completed or was intentionally skipped.</summary>
    public bool IsSuccess { get; init; }
    /// <summary>Gets whether the caller canceled the batch.</summary>
    public bool IsCanceled { get; init; }
    /// <summary>Gets the source bytes consumed by all started items.</summary>
    public long BytesTransferred { get; init; }
}
