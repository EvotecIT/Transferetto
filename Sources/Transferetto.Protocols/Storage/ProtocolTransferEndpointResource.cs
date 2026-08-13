using System;
using System.IO;
using System.Threading;
using Transferetto.Core;

namespace Transferetto;

internal static class ProtocolTransferEndpointResource {
    internal static TransferReadHandle OpenRead(
        Func<Stream> openStream,
        TransferItem item,
        CancellationToken cancellationToken) {
        Stream? stream = null;
        try {
            stream = openStream();
            cancellationToken.ThrowIfCancellationRequested();
            TransferReadHandle handle = new(item, stream);
            stream = null;
            return handle;
        } finally {
            stream?.Dispose();
        }
    }
}
