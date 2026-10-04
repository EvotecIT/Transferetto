namespace Transferetto.Core;

/// <summary>Identifies endpoints sharing a connection that requires coordinated transfer access.</summary>
public interface ITransferSessionEndpoint {
    /// <summary>Gets the shared connection identity. All wrappers over one connection must return the same object.</summary>
    object SessionKey { get; }
}
