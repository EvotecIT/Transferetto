using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Transferetto.Core;

namespace Transferetto.AzureBlob;

public sealed partial class AzureBlobTransferEndpoint {
    /// <inheritdoc />
    public string ResumableIdentity => DisplayName;

    /// <inheritdoc />
    public Task<string> BeginResumableWriteAsync(string path, TransferWriteOptions options,
        CancellationToken cancellationToken = default) {
        if (options == null) { throw new ArgumentNullException(nameof(options)); }
        ResolveName(path);
        return Task.FromResult(Guid.NewGuid().ToString("N"));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransferResumablePart>> ListResumablePartsAsync(string path, string sessionId,
        CancellationToken cancellationToken = default) {
        BlockBlobClient blob = _container.GetBlockBlobClient(ResolveName(path));
        try {
            Response<BlockList> response = await blob.GetBlockListAsync(BlockListTypes.Uncommitted,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            string marker = sessionId + ":";
            List<TransferResumablePart> parts = new();
            foreach (BlobBlock block in response.Value.UncommittedBlocks) {
                string decoded;
                try { decoded = Encoding.ASCII.GetString(Convert.FromBase64String(block.Name)); }
                catch (FormatException) { continue; }
                if (!decoded.StartsWith(marker, StringComparison.Ordinal)
                    || !int.TryParse(decoded.Substring(marker.Length), NumberStyles.None,
                        CultureInfo.InvariantCulture, out int number) || number < 1) { continue; }
                parts.Add(new TransferResumablePart {
                    Number = number, Length = block.SizeLong, Token = block.Name
                });
            }
            return parts;
        } catch (RequestFailedException exception) when (exception.Status == 404) {
            return Array.Empty<TransferResumablePart>();
        }
    }

    /// <inheritdoc />
    public async Task<TransferResumablePart> WriteResumablePartAsync(string path, string sessionId, int partNumber,
        Stream content, long length, CancellationToken cancellationToken = default) {
        if (partNumber < 1 || partNumber > 50000) { throw new ArgumentOutOfRangeException(nameof(partNumber)); }
        if (length <= 0 || length > 64L * 1024 * 1024) { throw new ArgumentOutOfRangeException(nameof(length)); }
        string token = PartToken(sessionId, partNumber);
        using MemoryStream buffered = new();
        await TransferContent.CopyToAsync(content, buffered, length, cancellationToken).ConfigureAwait(false);
        buffered.Position = 0;
        await _container.GetBlockBlobClient(ResolveName(path)).StageBlockAsync(token, buffered,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return new TransferResumablePart { Number = partNumber, Length = length, Token = token };
    }

    /// <inheritdoc />
    public async Task<TransferWriteResult> CompleteResumableWriteAsync(string path, string sessionId,
        IReadOnlyList<TransferResumablePart> parts, long length, TransferWriteOptions options,
        CancellationToken cancellationToken = default) {
        if (parts == null || parts.Count == 0) { throw new ArgumentException("At least one part is required.", nameof(parts)); }
        if (options == null) { throw new ArgumentNullException(nameof(options)); }
        TransferItem? existing = options.Mode == TransferWriteMode.Overwrite ? null
            : await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
        if (existing != null) {
            if (options.Mode == TransferWriteMode.SkipIfExists) { return new TransferWriteResult(existing, false); }
            throw new IOException($"The destination blob already exists: {path}");
        }
        BlockBlobClient blob = _container.GetBlockBlobClient(ResolveName(path));
        CommitBlockListOptions commit = new() {
            HttpHeaders = string.IsNullOrWhiteSpace(options.ContentType) ? null
                : new BlobHttpHeaders { ContentType = options.ContentType },
            Metadata = TransferMetadata.CopyPortable(options.Metadata),
            Conditions = options.Mode == TransferWriteMode.Overwrite ? null
                : new BlobRequestConditions { IfNoneMatch = ETag.All }
        };
        try {
            await blob.CommitBlockListAsync(parts.OrderBy(part => part.Number).Select(part => part.Token),
                commit, cancellationToken).ConfigureAwait(false);
            TransferItem item = await GetItemAsync(path, cancellationToken).ConfigureAwait(false)
                ?? throw new IOException("The committed blob could not be inspected.");
            if (item.Length != length) { throw new IOException("The committed blob length differs from the staged content."); }
            return new TransferWriteResult(item, true);
        } catch (RequestFailedException exception) when ((exception.Status == 409 || exception.Status == 412)
            && options.Mode == TransferWriteMode.SkipIfExists) {
            existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null) { return new TransferWriteResult(existing, false); }
            throw;
        } catch (RequestFailedException exception) when ((exception.Status == 409 || exception.Status == 412)
            && options.Mode == TransferWriteMode.FailIfExists) {
            throw new IOException($"The destination blob already exists: {path}", exception);
        }
    }

    /// <inheritdoc />
    public Task AbortResumableWriteAsync(string path, string sessionId,
        CancellationToken cancellationToken = default) {
        // Azure has no uncommitted-block deletion API. Blocks expire server-side; deleting the blob
        // here could destroy the existing committed destination.
        ResolveName(path);
        return Task.CompletedTask;
    }

    private static string PartToken(string sessionId, int number) => Convert.ToBase64String(
        Encoding.ASCII.GetBytes(sessionId + ":" + number.ToString("D5", CultureInfo.InvariantCulture)));
}
