using System;
using System.Collections.Generic;

namespace Transferetto.Core;

/// <summary>
/// Describes an opaque item exposed by a transfer endpoint.
/// </summary>
public sealed class TransferItem {
    /// <summary>Gets the endpoint-relative item path or object key.</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Gets the content length when known.</summary>
    public long? Length { get; init; }

    /// <summary>Gets the last modification time when supplied by the provider.</summary>
    public DateTimeOffset? LastModifiedUtc { get; init; }

    /// <summary>Gets the provider entity tag when available.</summary>
    public string? ETag { get; init; }

    /// <summary>Gets the provider version identifier when available.</summary>
    public string? VersionId { get; init; }

    /// <summary>Gets the media type when supplied by the provider.</summary>
    public string? ContentType { get; init; }

    /// <summary>Gets provider metadata associated with the item.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
