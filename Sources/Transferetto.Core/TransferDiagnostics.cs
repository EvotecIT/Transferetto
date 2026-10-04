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
    private static readonly Counter<long> Operations = Meter.CreateCounter<long>("transferetto.operations", "{operation}");
    private static readonly Histogram<double> OperationDuration = Meter.CreateHistogram<double>("transferetto.operation.duration", "s");
    private static readonly Counter<long> OperationBytes = Meter.CreateCounter<long>("transferetto.operation.bytes", "By");
    private static readonly Counter<long> CleanupFailures = Meter.CreateCounter<long>("transferetto.cleanup.failures", "{failure}");

    /// <summary>Starts a connection attempt without recording its host or credentials.</summary>
    public static OperationScope StartConnection(string scheme) => new("connection", scheme, "connect");

    /// <summary>Starts a named protocol operation without recording item paths or command text.</summary>
    public static OperationScope StartProtocolOperation(string scheme, string action) => new("protocol", scheme, action);

    /// <summary>Records a cleanup failure while preserving the original operation exception.</summary>
    public static void RecordCleanupFailure(string scheme, Exception exception) {
        try { CleanupFailures.Add(1, new KeyValuePair<string, object?>("transfer.scheme", scheme),
            new KeyValuePair<string, object?>("error.type", exception.GetType().FullName)); }
        catch { /* Observers must not change cleanup behavior. */ }
    }

    /// <summary>Records the outcome and duration of a connection or protocol operation.</summary>
    public sealed class OperationScope : IDisposable {
        private readonly Stopwatch _elapsed = Stopwatch.StartNew();
        private readonly Activity? _activity;
        private readonly string _kind;
        private readonly string _scheme;
        private readonly string _action;
        private string _outcome = "failed";
        private long _bytes;

        internal OperationScope(string kind, string scheme, string action) {
            _kind = kind;
            _scheme = scheme;
            _action = action;
            try {
                _activity = ActivitySource.StartActivity(kind == "connection" ? "transfer.connection" : "transfer.protocol");
                _activity?.SetTag("transfer.scheme", scheme);
                _activity?.SetTag("transfer.action", action);
            } catch { }
        }

        /// <summary>Marks the operation successful and records transferred bytes when known.</summary>
        public void Complete(long bytes = 0) { _outcome = "success"; _bytes = Math.Max(0, bytes); }

        /// <summary>Marks a failure by exception type without recording the message.</summary>
        public void Fail(Exception exception) {
            _outcome = exception is OperationCanceledException ? "canceled" : "failed";
            try {
                _activity?.SetStatus(ActivityStatusCode.Error);
                _activity?.SetTag("error.type", exception.GetType().FullName);
            } catch { }
        }

        /// <inheritdoc />
        public void Dispose() {
            try {
                _activity?.SetTag("transfer.outcome", _outcome);
                _activity?.SetTag("transfer.bytes", _bytes);
                _activity?.Dispose();
                KeyValuePair<string, object?>[] tags = {
                    new("transfer.kind", _kind), new("transfer.scheme", _scheme),
                    new("transfer.action", _action), new("transfer.outcome", _outcome)
                };
                Operations.Add(1, tags);
                OperationDuration.Record(_elapsed.Elapsed.TotalSeconds, tags);
                OperationBytes.Add(_bytes, tags);
            } catch { }
        }
    }

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
