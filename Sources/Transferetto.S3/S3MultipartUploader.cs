using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Transferetto.Core;

namespace Transferetto.S3;

internal static class S3MultipartUploader {
    private const long MaximumSinglePutBytes = 5L * 1024 * 1024 * 1024;
    private const long MaximumObjectBytes = 5L * 1024 * 1024 * 1024 * 1024;
    private const int MaximumParts = 10_000;
    private const int DefaultPartBytes = 64 * 1024 * 1024;
    private const int PartSizeAlignmentBytes = 1024 * 1024;

    internal static bool RequiresMultipartUpload(long? length) =>
        !length.HasValue || length.Value > MaximumSinglePutBytes;

    internal static async Task<S3ObjectWriteResult?> UploadAsync(
        IAmazonS3 client,
        string bucketName,
        string key,
        Stream content,
        long? length,
        TransferWriteOptions options,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken) {
        int partSize = CalculatePartSize(length);
        InitiateMultipartUploadRequest initiateRequest = new() {
            BucketName = bucketName,
            Key = key,
            ContentType = options.ContentType
        };
        foreach (KeyValuePair<string, string> pair in metadata) {
            initiateRequest.Metadata[pair.Key] = pair.Value;
        }

        InitiateMultipartUploadResponse initiated = await client
            .InitiateMultipartUploadAsync(initiateRequest, cancellationToken)
            .ConfigureAwait(false);
        bool completed = false;
        try {
            List<PartETag> parts = new();
            long bytesWritten = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(partSize);
            try {
                int partNumber = 1;
                while (true) {
                    int count = await ReadPartAsync(content, buffer, partSize, cancellationToken)
                        .ConfigureAwait(false);
                    if (count == 0) {
                        break;
                    }
                    if (partNumber > MaximumParts) {
                        throw new IOException(
                            $"The S3 multipart upload exceeded the {MaximumParts} part service limit.");
                    }

                    using MemoryStream partStream = new(buffer, 0, count, writable: false, publiclyVisible: true);
                    UploadPartResponse uploaded = await client.UploadPartAsync(new UploadPartRequest {
                        BucketName = bucketName,
                        Key = key,
                        UploadId = initiated.UploadId,
                        PartNumber = partNumber,
                        PartSize = count,
                        InputStream = partStream,
                        IsLastPart = length.HasValue && bytesWritten + count >= length.Value
                    }, cancellationToken).ConfigureAwait(false);
                    parts.Add(new PartETag {
                        PartNumber = partNumber,
                        ETag = uploaded.ETag
                    });
                    bytesWritten += count;
                    partNumber++;
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            if (parts.Count == 0) {
                return null;
            }

            CompleteMultipartUploadRequest completeRequest = new() {
                BucketName = bucketName,
                Key = key,
                UploadId = initiated.UploadId,
                PartETags = parts,
                MpuObjectSize = bytesWritten,
                IfNoneMatch = options.Mode == TransferWriteMode.Overwrite ? null : "*"
            };
            CompleteMultipartUploadResponse response = await client
                .CompleteMultipartUploadAsync(completeRequest, cancellationToken)
                .ConfigureAwait(false);
            completed = true;
            return new S3ObjectWriteResult(response.ETag, response.VersionId, bytesWritten);
        } finally {
            if (!completed) {
                try {
                    await client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest {
                        BucketName = bucketName,
                        Key = key,
                        UploadId = initiated.UploadId
                    }, CancellationToken.None).ConfigureAwait(false);
                } catch {
                    // Preserve the original upload, cancellation, or collision failure.
                }
            }
        }
    }

    private static int CalculatePartSize(long? length) {
        if (length.HasValue && length.Value > MaximumObjectBytes) {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                $"S3 objects cannot exceed {MaximumObjectBytes} bytes.");
        }
        if (!length.HasValue || length.Value <= 0) {
            return DefaultPartBytes;
        }

        long minimum = (length.Value + MaximumParts - 1) / MaximumParts;
        long aligned = ((minimum + PartSizeAlignmentBytes - 1) / PartSizeAlignmentBytes) *
                       PartSizeAlignmentBytes;
        long selected = Math.Max(DefaultPartBytes, aligned);
        if (selected > int.MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(length), "The required S3 part size is too large.");
        }
        return (int)selected;
    }

    private static async Task<int> ReadPartAsync(
        Stream content,
        byte[] buffer,
        int count,
        CancellationToken cancellationToken) {
        int total = 0;
        while (total < count) {
            int read = await content.ReadAsync(buffer, total, count - total, cancellationToken)
                .ConfigureAwait(false);
            if (read == 0) {
                break;
            }
            total += read;
        }
        return total;
    }
}

internal sealed class S3ObjectWriteResult {
    internal S3ObjectWriteResult(string? eTag, string? versionId, long? length) {
        ETag = eTag;
        VersionId = versionId;
        Length = length;
    }

    internal string? ETag { get; }
    internal string? VersionId { get; }
    internal long? Length { get; }
}
