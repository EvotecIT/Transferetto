using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>Optionally copies provider objects without relaying their content through the caller.</summary>
public interface ITransferServerSideCopyEndpoint {
    /// <summary>Returns null when this source, size, or authentication cannot use provider-side copy.</summary>
    /// <remarks>A returned result means the provider completed the copy. Implementations must enforce the
    /// source version and destination write mode; a failed request must throw rather than return null.</remarks>
    Task<TransferWriteResult?> TryCopyServerSideAsync(ITransferEndpoint source, string sourcePath,
        TransferItem sourceItem, string destinationPath, TransferWriteOptions writeOptions,
        CancellationToken cancellationToken = default);
}
