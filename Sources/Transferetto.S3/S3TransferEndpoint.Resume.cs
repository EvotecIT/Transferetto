using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Transferetto.Core;

namespace Transferetto.S3;

public sealed partial class S3TransferEndpoint {
    /// <inheritdoc />
    public string ResumableIdentity => DisplayName + "|" + ServiceIdentity()
        + "|" + (_client.Config.RegionEndpoint?.SystemName ?? string.Empty);

    private string ServiceIdentity() {
        if (!Uri.TryCreate(_client.Config.ServiceURL, UriKind.Absolute, out Uri? uri)) { return string.Empty; }
        UriBuilder safe = new(uri) { UserName = string.Empty, Password = string.Empty,
            Query = string.Empty, Fragment = string.Empty };
        return safe.Uri.GetLeftPart(UriPartial.Path);
    }

    /// <inheritdoc />
    public async Task<string> BeginResumableWriteAsync(string path, TransferWriteOptions options,
        CancellationToken cancellationToken = default) {
        if (options == null) { throw new ArgumentNullException(nameof(options)); }
        InitiateMultipartUploadRequest request = new() {
            BucketName = _bucketName, Key = ResolveKey(path), ContentType = options.ContentType
        };
        foreach (KeyValuePair<string, string> pair in TransferMetadata.CopyPortable(options.Metadata)) {
            request.Metadata[pair.Key] = pair.Value;
        }
        InitiateMultipartUploadResponse response = await _client.InitiateMultipartUploadAsync(request,
            cancellationToken).ConfigureAwait(false);
        return response.UploadId;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransferResumablePart>> ListResumablePartsAsync(string path, string sessionId,
        CancellationToken cancellationToken = default) {
        List<TransferResumablePart> parts = new();
        string? marker = null;
        do {
            ListPartsResponse response = await _client.ListPartsAsync(new ListPartsRequest {
                BucketName = _bucketName, Key = ResolveKey(path), UploadId = sessionId,
                PartNumberMarker = marker
            }, cancellationToken).ConfigureAwait(false);
            foreach (PartDetail part in response.Parts ?? new List<PartDetail>()) {
                if (part.PartNumber is not int number || part.Size is not long size || number < 1 || size < 0) {
                    throw new InvalidDataException("The S3 service returned an invalid staged part.");
                }
                parts.Add(new TransferResumablePart { Number = number, Length = size, Token = part.ETag });
            }
            if (response.IsTruncated != true) { break; }
            string? nextMarker = response.NextPartNumberMarker?.ToString(CultureInfo.InvariantCulture);
            if (nextMarker == marker || nextMarker == null) {
                throw new IOException("The S3 service did not advance the multipart listing cursor.");
            }
            marker = nextMarker;
        } while (true);
        return parts;
    }

    /// <inheritdoc />
    public async Task<TransferResumablePart> WriteResumablePartAsync(string path, string sessionId, int partNumber,
        Stream content, long length, CancellationToken cancellationToken = default) {
        if (partNumber < 1 || partNumber > 10000) { throw new ArgumentOutOfRangeException(nameof(partNumber)); }
        if (length <= 0 || length > 64L * 1024 * 1024) { throw new ArgumentOutOfRangeException(nameof(length)); }
        using MemoryStream buffered = new();
        await TransferContent.CopyToAsync(content, buffered, length, cancellationToken).ConfigureAwait(false);
        buffered.Position = 0;
        UploadPartResponse response = await _client.UploadPartAsync(new UploadPartRequest {
            BucketName = _bucketName, Key = ResolveKey(path), UploadId = sessionId,
            PartNumber = partNumber, PartSize = length, InputStream = buffered,
            IsLastPart = length < 5L * 1024 * 1024
        }, cancellationToken).ConfigureAwait(false);
        return new TransferResumablePart { Number = partNumber, Length = length, Token = response.ETag };
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
            throw new IOException($"The destination object already exists: {path}");
        }
        try {
            CompleteMultipartUploadResponse response = await _client.CompleteMultipartUploadAsync(
                new CompleteMultipartUploadRequest {
                    BucketName = _bucketName, Key = ResolveKey(path), UploadId = sessionId,
                    PartETags = parts.OrderBy(part => part.Number).Select(part => new PartETag {
                        PartNumber = part.Number, ETag = part.Token
                    }).ToList(),
                    MpuObjectSize = length,
                    IfNoneMatch = options.Mode == TransferWriteMode.Overwrite ? null : "*"
                }, cancellationToken).ConfigureAwait(false);
            return new TransferWriteResult(new TransferItem {
                Path = path, Length = length, LastModifiedUtc = DateTimeOffset.UtcNow,
                ETag = TrimETag(response.ETag), VersionId = response.VersionId,
                ContentType = options.ContentType,
                Metadata = TransferMetadata.CopyPortable(options.Metadata)
            }, true);
        } catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.PreconditionFailed
            && options.Mode == TransferWriteMode.SkipIfExists) {
            existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null) { return new TransferWriteResult(existing, false); }
            throw;
        } catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.PreconditionFailed
            && options.Mode == TransferWriteMode.FailIfExists) {
            throw new IOException($"The destination object already exists: {path}", exception);
        }
    }

    /// <inheritdoc />
    public async Task AbortResumableWriteAsync(string path, string sessionId,
        CancellationToken cancellationToken = default) {
        await _client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest {
            BucketName = _bucketName, Key = ResolveKey(path), UploadId = sessionId
        }, cancellationToken).ConfigureAwait(false);
    }
}
