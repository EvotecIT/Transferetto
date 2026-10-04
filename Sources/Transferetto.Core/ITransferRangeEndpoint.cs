using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>Optionally opens a bounded source range tied to a previously inspected item.</summary>
public interface ITransferRangeEndpoint {
    /// <summary>Opens exactly the requested range or throws if the source identity changed.</summary>
    Task<TransferReadHandle> OpenReadRangeAsync(string path, long offset, long length,
        TransferItem expectedItem, CancellationToken cancellationToken = default);
}
