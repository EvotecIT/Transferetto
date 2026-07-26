using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Provides provider-neutral access to opaque file or object content.
/// </summary>
public interface ITransferEndpoint {
    /// <summary>Gets the provider scheme, such as <c>file</c>, <c>s3</c>, or <c>azureblob</c>.</summary>
    string Scheme { get; }

    /// <summary>Gets a safe endpoint description that contains no credentials.</summary>
    string DisplayName { get; }

    /// <summary>Gets the operations implemented by the endpoint.</summary>
    TransferEndpointCapabilities Capabilities { get; }

    /// <summary>Inspects one item, returning <see langword="null"/> when it does not exist.</summary>
    Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Lists items beneath a prefix.</summary>
    Task<IReadOnlyList<TransferItem>> ListAsync(
        string prefix,
        bool recursive = true,
        CancellationToken cancellationToken = default);

    /// <summary>Opens an item for streaming reads.</summary>
    Task<TransferReadHandle> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Writes an item from a readable stream.</summary>
    Task<TransferWriteResult> WriteAsync(
        string path,
        Stream content,
        long? length,
        TransferWriteOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an item and returns whether it existed.</summary>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
}
