using System;
using System.IO;

namespace Transferetto.Core;

/// <summary>
/// Owns a readable item stream and its provider metadata.
/// </summary>
public sealed class TransferReadHandle : IDisposable {
    /// <summary>Initializes a readable transfer handle.</summary>
    public TransferReadHandle(TransferItem item, Stream stream) {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>Gets the source item metadata.</summary>
    public TransferItem Item { get; }

    /// <summary>Gets the readable content stream.</summary>
    public Stream Stream { get; }

    /// <inheritdoc />
    public void Dispose() => Stream.Dispose();
}
