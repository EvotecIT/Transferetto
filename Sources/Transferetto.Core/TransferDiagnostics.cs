using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Transferetto.Core;

/// <summary>Exposes transfer tracing and metrics without recording paths, credentials, or provider URLs.</summary>
public static class TransferDiagnostics {
    /// <summary>Gets the instrumentation name used by both tracing and metrics.</summary>
    public const string Name = "Transferetto.Core";
    /// <summary>Gets the activity source for transfer spans.</summary>
    public static ActivitySource ActivitySource { get; } = new(Name);
    /// <summary>Gets the meter publishing transfer counts, transferred bytes, and duration.</summary>
    public static Meter Meter { get; } = new(Name);
    private static readonly Counter<long> Transfers = Meter.CreateCounter<long>("transferetto.transfers", "{transfer}");
    private static readonly Counter<long> Bytes = Meter.CreateCounter<long>("transferetto.bytes", "By");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("transferetto.duration", "s");

    internal sealed class Operation : IDisposable {
        private readonly Stopwatch _elapsed = Stopwatch.StartNew();
        private readonly Activity? _activity;
        private readonly string _source;
        private readonly string _destination;
        private string _outcome = "failed";
        private long _bytes;

        internal Operation(ITransferEndpoint source, ITransferEndpoint destination, Guid correlationId) {
            _source = source.Scheme;
            _destination = destination.Scheme;
            try {
                _activity = ActivitySource.StartActivity("transfer.copy", ActivityKind.Internal);
                _activity?.SetTag("transfer.correlation_id", correlationId.ToString("D"));
                _activity?.SetTag("transfer.source.scheme", _source);
                _activity?.SetTag("transfer.destination.scheme", _destination);
            } catch { /* Instrumentation observers must not change transfer behavior. */ }
        }

        internal void Complete(TransferReceipt receipt) {
            _outcome = receipt.Outcome == TransferReceiptOutcome.Copied ? "copied" : "skipped";
            _bytes = receipt.BytesTransferred;
        }

        internal void Fail(Exception exception) {
            _outcome = exception is OperationCanceledException ? "canceled" : "failed";
            try {
                _activity?.SetStatus(ActivityStatusCode.Error);
                _activity?.SetTag("error.type", exception.GetType().FullName);
            } catch { }
        }

        public void Dispose() {
            try {
                _activity?.SetTag("transfer.outcome", _outcome);
                _activity?.SetTag("transfer.bytes", _bytes);
                _activity?.Dispose();
                KeyValuePair<string, object?>[] tags = {
                    new("transfer.source.scheme", _source), new("transfer.destination.scheme", _destination),
                    new("transfer.outcome", _outcome)
                };
                Transfers.Add(1, tags);
                Bytes.Add(_bytes, tags);
                Duration.Record(_elapsed.Elapsed.TotalSeconds, tags);
            } catch { /* Instrumentation observers must not change transfer behavior. */ }
        }
    }
}
