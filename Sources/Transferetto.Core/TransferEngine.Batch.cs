using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

public static partial class TransferEngine {
    /// <summary>Copies multiple items using bounded workers and results in input order.</summary>
    /// <remarks>Session-backed endpoints are serialized by connection identity. Cancellation returns settled and
    /// not-started results so callers can inspect completed siblings. Pass separate FTP connections for source
    /// and destination when copying between FTP endpoints.</remarks>
    public static async Task<TransferBatchResult> CopyBatchAsync(IReadOnlyList<TransferBatchItem> items,
        TransferBatchOptions? options = null, CancellationToken cancellationToken = default) {
        if (items == null) { throw new ArgumentNullException(nameof(items)); }
        TransferBatchOptions resolved = options ?? new TransferBatchOptions();
        if (resolved.MaxConcurrency < 1 || resolved.MaxConcurrency > 256) {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxConcurrency must be between 1 and 256.");
        }
        TransferBatchItem[] requests = items.ToArray();
        if (requests.Any(item => item == null)) { throw new ArgumentException("Batch items cannot be null.", nameof(items)); }
        TransferBatchItemResult?[] settled = new TransferBatchItemResult[requests.Length];
        long[] observed = new long[requests.Length];
        object progressSync = new();
        long totalBytes = 0;
        int completed = 0;
        int next = -1;
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        HashSet<string> destinations = new(StringComparer.Ordinal);
        foreach (TransferBatchItem item in requests) {
            if (resolved.CheckpointPath != null) {
                string checkpoint = System.IO.Path.GetFullPath(resolved.CheckpointPath);
                EnsureCheckpointNotItemPath(checkpoint, item.Source, item.SourcePath);
                EnsureCheckpointNotItemPath(checkpoint, item.Destination, item.DestinationPath);
            }
            // Resolve local aliases before scheduling concurrent writes to one file.
            string path = item.Destination is FileSystemTransferEndpoint local
                ? local.ResolveForResume(item.DestinationPath)
                : item.DestinationPath;
            path = item.Destination.Scheme == "file" &&
                          System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                              System.Runtime.InteropServices.OSPlatform.Windows)
                ? path.ToUpperInvariant()
                : path;
            string identity = item.Destination.DisplayName + "\n" + path;
            if (!destinations.Add(identity)) {
                throw new ArgumentException("A batch cannot schedule the same destination path more than once.", nameof(items));
            }
        }
        TransferBatchCheckpointStore? checkpoints = resolved.CheckpointPath == null ? null
            : new TransferBatchCheckpointStore(resolved.CheckpointPath);
        void Report(int index, long current, bool done) {
            lock (progressSync) {
                long delta = Math.Max(0, current - observed[index]);
                observed[index] += delta;
                totalBytes = checked(totalBytes + delta);
                if (done) { completed++; }
                TransferBatchProgress snapshot = new() { TotalItems = requests.Length,
                    CompletedItems = completed, BytesTransferred = totalBytes };
                try { resolved.Progress?.Report(snapshot); }
                catch { /* An observer must not discard completed sibling results. */ }
            }
        }
        async Task WorkerAsync() {
            while (!linked.IsCancellationRequested) {
                int index = Interlocked.Increment(ref next);
                if (index >= requests.Length) { return; }
                TransferBatchItem item = requests[index];
                if (linked.IsCancellationRequested) { return; }
                TransferCopyOptions itemOptions = item.Options ?? new TransferCopyOptions();
                IProgress<TransferProgress>? original = itemOptions.Progress;
                TransferCopyOptions effective = new() {
                    WriteOptions = itemOptions.WriteOptions,
                    ExpectedSha256 = itemOptions.ExpectedSha256,
                    VerifyDestination = itemOptions.VerifyDestination,
                    PreferServerSideCopy = itemOptions.PreferServerSideCopy,
                    ProgressIntervalBytes = itemOptions.ProgressIntervalBytes,
                    Progress = new InlineProgress<TransferProgress>(value => {
                        try { original?.Report(value); }
                        catch { /* A progress observer cannot invalidate a copy. */ }
                        Report(index, value.BytesTransferred, false);
                    })
                };
                TransferReceipt? receipt = null;
                Exception? error = null;
                TransferBatchItemOutcome outcome;
                try {
                    if (checkpoints != null && await checkpoints.TryResumeAsync(index, item, linked.Token).ConfigureAwait(false)) {
                        outcome = TransferBatchItemOutcome.Resumed;
                    } else {
                        receipt = await CopyAsync(item.Source, item.SourcePath, item.Destination, item.DestinationPath,
                            effective, linked.Token).ConfigureAwait(false);
                        outcome = receipt.Outcome == TransferReceiptOutcome.Copied
                            ? TransferBatchItemOutcome.Copied : TransferBatchItemOutcome.Skipped;
                        if (receipt.Outcome == TransferReceiptOutcome.Copied && checkpoints != null) {
                            await checkpoints.RecordAsync(index, item, receipt, linked.Token).ConfigureAwait(false);
                        }
                    }
                } catch (OperationCanceledException exception) when (linked.IsCancellationRequested) {
                    error = exception;
                    outcome = TransferBatchItemOutcome.Canceled;
                } catch (Exception exception) {
                    error = exception;
                    outcome = TransferBatchItemOutcome.Failed;
                    if (resolved.FailFast) { linked.Cancel(); }
                }
                settled[index] = new TransferBatchItemResult { Index = index, Item = item,
                    Receipt = receipt, Error = error, Outcome = outcome };
                Report(index, receipt?.BytesTransferred ?? observed[index], true);
            }
        }
        Task[] workers = new Task[Math.Min(resolved.MaxConcurrency, requests.Length)];
        for (int index = 0; index < workers.Length; index++) { workers[index] = WorkerAsync(); }
        await Task.WhenAll(workers).ConfigureAwait(false);
        TransferBatchItemResult[] results = new TransferBatchItemResult[requests.Length];
        for (int index = 0; index < requests.Length; index++) {
            results[index] = settled[index] ?? new TransferBatchItemResult {
                Index = index, Item = requests[index], Outcome = TransferBatchItemOutcome.NotStarted
            };
        }
        return new TransferBatchResult { Items = results, BytesTransferred = totalBytes,
            IsCanceled = cancellationToken.IsCancellationRequested,
            IsSuccess = results.All(result => result.Outcome == TransferBatchItemOutcome.Copied
                || result.Outcome == TransferBatchItemOutcome.Skipped || result.Outcome == TransferBatchItemOutcome.Resumed) };
    }

    private sealed class InlineProgress<T> : IProgress<T> {
        private readonly Action<T> _report;
        internal InlineProgress(Action<T> report) { _report = report; }
        public void Report(T value) => _report(value);
    }
}
