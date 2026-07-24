using System;

namespace Transferetto.Core;

/// <summary>
/// Records the verified outcome of a provider-neutral transfer.
/// </summary>
public sealed class TransferReceipt {
    /// <summary>Gets a correlation identifier for the transfer.</summary>
    public Guid CorrelationId { get; init; }

    /// <summary>Gets the source endpoint description.</summary>
    public string SourceEndpoint { get; init; } = string.Empty;

    /// <summary>Gets the source item path.</summary>
    public string SourcePath { get; init; } = string.Empty;

    /// <summary>Gets the destination endpoint description.</summary>
    public string DestinationEndpoint { get; init; } = string.Empty;

    /// <summary>Gets the destination item path.</summary>
    public string DestinationPath { get; init; } = string.Empty;

    /// <summary>Gets how the transfer request settled.</summary>
    public TransferReceiptOutcome Outcome { get; init; }

    /// <summary>Gets the number of bytes consumed from the source.</summary>
    public long BytesTransferred { get; init; }

    /// <summary>Gets the SHA-256 digest of the transferred byte stream.</summary>
    public string? Sha256 { get; init; }

    /// <summary>Gets the provider entity tag observed at the source.</summary>
    public string? SourceETag { get; init; }

    /// <summary>Gets the provider entity tag returned by the destination.</summary>
    public string? DestinationETag { get; init; }

    /// <summary>Gets the UTC time at which the transfer started.</summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>Gets the UTC time at which the transfer completed.</summary>
    public DateTimeOffset CompletedAtUtc { get; init; }
}
