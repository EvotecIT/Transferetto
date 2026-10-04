namespace Transferetto.Core;

/// <summary>Exposes the path identity used by an endpoint after applying its own normalization rules.</summary>
/// <remarks>Implement this when distinct input spellings can address the same item. Batches use the identity
/// to reject concurrent writes to one destination before any item starts.</remarks>
public interface ITransferPathIdentityEndpoint {
    /// <summary>Returns the canonical identity of an endpoint-relative item path.</summary>
    string GetPathIdentity(string path);
}
