using System;

namespace Transferetto.Core;

/// <summary>
/// Identifies the data-plane operations implemented by a transfer endpoint.
/// </summary>
[Flags]
public enum TransferEndpointCapabilities {
    /// <summary>No endpoint operations are available.</summary>
    None = 0,
    /// <summary>Individual items can be inspected.</summary>
    Inspect = 1,
    /// <summary>Items can be enumerated.</summary>
    List = 2,
    /// <summary>Item content can be read.</summary>
    Read = 4,
    /// <summary>Item content can be written.</summary>
    Write = 8,
    /// <summary>Items can be deleted.</summary>
    Delete = 16,
    /// <summary>Provider metadata can be stored with an item.</summary>
    Metadata = 32,
    /// <summary>The provider exposes a stable item version or entity tag.</summary>
    Versioning = 64
}
