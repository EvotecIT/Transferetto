using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>Optional provider multipart or staged-block write operations used by the Core resume engine.</summary>
public interface ITransferResumableWriteEndpoint {
    /// <summary>Gets a credential-free service and namespace identity for checkpoint validation.</summary>
    string ResumableIdentity { get; }
    /// <summary>Starts a provider-side staging session and returns its opaque identifier.</summary>
    Task<string> BeginResumableWriteAsync(string path, TransferWriteOptions options,
        CancellationToken cancellationToken = default);
    /// <summary>Lists parts still staged for a previous session.</summary>
    Task<IReadOnlyList<TransferResumablePart>> ListResumablePartsAsync(string path, string sessionId,
        CancellationToken cancellationToken = default);
    /// <summary>Stages one complete part and returns its provider token.</summary>
    Task<TransferResumablePart> WriteResumablePartAsync(string path, string sessionId, int partNumber,
        Stream content, long length, CancellationToken cancellationToken = default);
    /// <summary>Atomically commits the ordered staged parts according to destination policy.</summary>
    Task<TransferWriteResult> CompleteResumableWriteAsync(string path, string sessionId,
        IReadOnlyList<TransferResumablePart> parts, long length, TransferWriteOptions options,
        CancellationToken cancellationToken = default);
    /// <summary>Discards an abandoned staging session.</summary>
    Task AbortResumableWriteAsync(string path, string sessionId, CancellationToken cancellationToken = default);
}

/// <summary>Identifies one complete provider-staged part.</summary>
public sealed class TransferResumablePart {
    /// <summary>Gets the one-based part number.</summary>
    public int Number { get; init; }
    /// <summary>Gets the byte count of the staged part.</summary>
    public long Length { get; init; }
    /// <summary>Gets the provider token required to commit the part.</summary>
    public string Token { get; init; } = string.Empty;
}
