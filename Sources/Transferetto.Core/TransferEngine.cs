using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Streams content between any two compatible transfer endpoints.
/// </summary>
public static partial class TransferEngine {
    /// <summary>
    /// Copies one item between endpoints while calculating a provider-independent SHA-256 receipt.
    /// </summary>
    public static async Task<TransferReceipt> CopyAsync(
        ITransferEndpoint source,
        string sourcePath,
        ITransferEndpoint destination,
        string destinationPath,
        TransferCopyOptions? options = null,
        CancellationToken cancellationToken = default) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination));
        }
        if (string.IsNullOrEmpty(sourcePath)) {
            throw new ArgumentException("A source path is required.", nameof(sourcePath));
        }
        if (string.IsNullOrEmpty(destinationPath)) {
            throw new ArgumentException("A destination path is required.", nameof(destinationPath));
        }

        TransferCopyOptions resolvedOptions = options ?? new TransferCopyOptions();
        string? expectedSha256 = NormalizeExpectedHash(resolvedOptions.ExpectedSha256);
        if (resolvedOptions.VerifyDestination && (destination.Capabilities & TransferEndpointCapabilities.Read) == 0) {
            throw new NotSupportedException("Destination verification requires a readable destination endpoint.");
        }
        DateTimeOffset startedAtUtc = DateTimeOffset.UtcNow;
        Guid correlationId = Guid.NewGuid();
        using TransferDiagnostics.Operation operation = new(source, destination, correlationId);
        try {
        using TransferEndpointLease lease = await TransferEndpointLease.AcquireAsync(source, destination, cancellationToken).ConfigureAwait(false);

        if (resolvedOptions.PreferServerSideCopy && expectedSha256 == null && !resolvedOptions.VerifyDestination
            && destination is ITransferServerSideCopyEndpoint nativeDestination) {
            TransferItem? inspected = await source.GetItemAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (inspected == null) { throw new FileNotFoundException("The source item does not exist.", sourcePath); }
            TransferWriteOptions nativeOptions = CloneWriteOptions(resolvedOptions.WriteOptions, inspected, destination.Capabilities);
            TransferWriteResult? nativeResult = await nativeDestination.TryCopyServerSideAsync(source, sourcePath,
                inspected, destinationPath, nativeOptions, cancellationToken).ConfigureAwait(false);
            if (nativeResult != null) {
                TransferReceipt nativeReceipt = new() {
                    CorrelationId = correlationId,
                    SourceEndpoint = source.DisplayName,
                    SourcePath = sourcePath,
                    DestinationEndpoint = destination.DisplayName,
                    DestinationPath = destinationPath,
                    Outcome = nativeResult.WasWritten ? TransferReceiptOutcome.Copied : TransferReceiptOutcome.Skipped,
                    BytesTransferred = nativeResult.WasWritten ? inspected.Length ?? 0 : 0,
                    ServerSideCopy = nativeResult.WasWritten,
                    SourceETag = inspected.ETag,
                    DestinationETag = nativeResult.Item.ETag,
                    StartedAtUtc = startedAtUtc,
                    CompletedAtUtc = DateTimeOffset.UtcNow
                };
                operation.Complete(nativeReceipt);
                return nativeReceipt;
            }
        }

        using TransferReadHandle readHandle = await source.OpenReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        long? sourceLength = NormalizeLength(readHandle.Item.Length);
        using ProgressHashingReadStream trackedStream = new(
            readHandle.Stream,
            sourcePath,
            destinationPath,
            sourceLength,
            resolvedOptions.Progress,
            resolvedOptions.ProgressIntervalBytes,
            expectedSha256);

        TransferWriteOptions writeOptions = CloneWriteOptions(
            resolvedOptions.WriteOptions,
            readHandle.Item,
            destination.Capabilities);
        if (sourceLength == 0) { trackedStream.Complete(); }
        TransferWriteResult writeResult = await destination.WriteAsync(
            destinationPath,
            trackedStream,
            sourceLength,
            writeOptions,
            cancellationToken).ConfigureAwait(false);

        if (writeResult.WasWritten) {
            trackedStream.Complete();
            if (sourceLength.HasValue && trackedStream.BytesRead != sourceLength.Value) {
                throw new EndOfStreamException(
                    $"The destination consumed {trackedStream.BytesRead} bytes but the source length is {sourceLength.Value}.");
            }
        }
        bool destinationVerified = writeResult.WasWritten && resolvedOptions.VerifyDestination;
        if (destinationVerified) {
            using TransferReadHandle verification = await destination.OpenReadAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            using ProgressHashingReadStream verifyStream = new(verification.Stream, sourcePath, destinationPath,
                trackedStream.BytesRead, null, 65536, trackedStream.Sha256);
            await TransferContent.CopyToAsync(verifyStream, Stream.Null, trackedStream.BytesRead, cancellationToken).ConfigureAwait(false);
        }
        TransferReceipt receipt = new() {
            CorrelationId = correlationId,
            SourceEndpoint = source.DisplayName,
            SourcePath = sourcePath,
            DestinationEndpoint = destination.DisplayName,
            DestinationPath = destinationPath,
            Outcome = writeResult.WasWritten ? TransferReceiptOutcome.Copied : TransferReceiptOutcome.Skipped,
            BytesTransferred = trackedStream.BytesRead,
            Sha256 = writeResult.WasWritten ? trackedStream.Sha256 : null,
            DestinationVerified = destinationVerified,
            SourceETag = readHandle.Item.ETag,
            DestinationETag = writeResult.Item.ETag,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };
        operation.Complete(receipt);
        return receipt;
        } catch (Exception exception) { operation.Fail(exception); throw; }
    }

    private static string? NormalizeExpectedHash(string? digest) {
        if (digest == null) { return null; }
        if (digest.Length != 64) { throw new ArgumentException("ExpectedSha256 must contain 64 hexadecimal characters.", nameof(digest)); }
        foreach (char character in digest) {
            if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f')
                || (character >= 'A' && character <= 'F'))) {
                throw new ArgumentException("ExpectedSha256 must contain 64 hexadecimal characters.", nameof(digest));
            }
        }
        return digest.ToLowerInvariant();
    }

    private static long? NormalizeLength(long? length) => length >= 0 ? length : null;

    private static TransferWriteOptions CloneWriteOptions(
        TransferWriteOptions options,
        TransferItem sourceItem,
        TransferEndpointCapabilities destinationCapabilities) {
        bool supportsMetadata = (destinationCapabilities & TransferEndpointCapabilities.Metadata) != 0;
        if (!supportsMetadata &&
            (!string.IsNullOrWhiteSpace(options.ContentType) || options.Metadata.Count > 0)) {
            throw new NotSupportedException(
                "The destination endpoint does not support explicitly requested content type or metadata.");
        }
        TransferWriteOptions clone = new() {
            Mode = options.Mode,
            ContentType = supportsMetadata ? options.ContentType ?? sourceItem.ContentType : null
        };
        if (supportsMetadata) {
            foreach (var pair in sourceItem.Metadata) {
                if (TransferMetadata.IsPortableName(pair.Key) && pair.Value != null) {
                    clone.Metadata[pair.Key] = pair.Value;
                }
            }
            foreach (var pair in options.Metadata) {
                clone.Metadata[pair.Key] = pair.Value;
            }
        }
        return clone;
    }

}
