using System;
using System.Collections.Generic;

namespace Transferetto.Core;

/// <summary>
/// Controls a write to a transfer endpoint.
/// </summary>
public sealed class TransferWriteOptions {
    /// <summary>Gets or sets the destination collision policy.</summary>
    public TransferWriteMode Mode { get; set; } = TransferWriteMode.FailIfExists;

    /// <summary>Gets or sets the destination media type.</summary>
    public string? ContentType { get; set; }

    /// <summary>Gets metadata to persist when the provider supports it.</summary>
    public IDictionary<string, string> Metadata { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
