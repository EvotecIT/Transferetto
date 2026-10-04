using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Transferetto.Core;

namespace Transferetto.AzureBlob;

internal static class AzureBlobVerifiedUploader {
    private const int MaximumBlocks = 50_000;
    private const int DefaultBlockBytes = 4 * 1024 * 1024;
    private const int MaximumBlockBytes = 128 * 1024 * 1024;

    internal static async Task<(Response<BlobContentInfo> Response, long Length)> UploadAsync(
        BlockBlobClient blob, Stream content, long? length, BlobUploadOptions options, CancellationToken cancellationToken) {
        long required = length > 0 ? ((length.Value - 1) / MaximumBlocks) + 1 : DefaultBlockBytes;
        if (required > MaximumBlockBytes) { throw new ArgumentOutOfRangeException(nameof(length), "The advertised blob requires blocks larger than the supported staging limit."); }
        int blockBytes = (int)Math.Max(DefaultBlockBytes, required);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(blockBytes);
        List<string> blocks = new();
        string uploadId = Guid.NewGuid().ToString("N");
        long bytesWritten = 0;
        try {
            while (true) {
                int read = await TransferContent.ReadBlockAsync(content, buffer, blockBytes, cancellationToken).ConfigureAwait(false);
                if (read == 0) { break; }
                long next = checked(bytesWritten + read);
                if (length >= 0 && next > length.Value) { throw new EndOfStreamException("The blob content exceeds its advertised length."); }
                if (blocks.Count == MaximumBlocks) { throw new IOException("The blob exceeds the maximum number of staging blocks."); }
                // Fixed-length identifiers isolate concurrent uploads to the same destination.
                string blockId = Convert.ToBase64String(Encoding.ASCII.GetBytes(uploadId + blocks.Count.ToString("D5")));
                using MemoryStream block = new(buffer, 0, read, writable: false, publiclyVisible: true);
                await blob.StageBlockAsync(blockId, block, cancellationToken: cancellationToken).ConfigureAwait(false);
                blocks.Add(blockId);
                bytesWritten = next;
            }
            if (length >= 0 && bytesWritten != length.Value) {
                throw new EndOfStreamException($"The blob content produced {bytesWritten} bytes but its expected length is {length.Value}.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            Response<BlobContentInfo> response = await blob.CommitBlockListAsync(blocks, new CommitBlockListOptions {
                Conditions = options.Conditions, HttpHeaders = options.HttpHeaders, Metadata = options.Metadata
            }, cancellationToken).ConfigureAwait(false);
            return (response, bytesWritten);
        } finally {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
            // Azure expires uncommitted blocks. Deleting here would also delete a pre-existing blob.
        }
    }
}
