using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

internal sealed class TransferEndpointLease : IDisposable {
    private static readonly ConditionalWeakTable<object, Gate> Gates = new();
    private static long _nextId;
    private readonly List<Gate> _held = new();

    internal static async Task<TransferEndpointLease> AcquireAsync(ITransferEndpoint source, ITransferEndpoint destination,
        CancellationToken cancellationToken) {
        object sourceKey = (source as ITransferSessionEndpoint)?.SessionKey ?? source;
        object destinationKey = (destination as ITransferSessionEndpoint)?.SessionKey ?? destination;
        if (ReferenceEquals(sourceKey, destinationKey) && (source.Scheme == "ftp" || source.Scheme == "ftps")) {
            throw new NotSupportedException("A streamed FTP copy requires separate source and destination connections.");
        }
        List<Gate> required = new();
        if ((source.Capabilities & TransferEndpointCapabilities.ConcurrentOperations) == 0) {
            required.Add(Gates.GetValue(sourceKey, _ => new Gate()));
        }
        if ((destination.Capabilities & TransferEndpointCapabilities.ConcurrentOperations) == 0) {
            Gate gate = Gates.GetValue(destinationKey, _ => new Gate());
            if (!required.Contains(gate)) { required.Add(gate); }
        }
        required.Sort((left, right) => left.Id.CompareTo(right.Id));
        TransferEndpointLease lease = new();
        try {
            foreach (Gate gate in required) {
                await gate.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                lease._held.Add(gate);
            }
            return lease;
        } catch { lease.Dispose(); throw; }
    }

    public void Dispose() {
        for (int index = _held.Count - 1; index >= 0; index--) { _held[index].Semaphore.Release(); }
        _held.Clear();
    }

    private sealed class Gate {
        internal readonly long Id = Interlocked.Increment(ref _nextId);
        internal readonly SemaphoreSlim Semaphore = new(1, 1);
    }
}
