using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Transferetto.Core;

namespace Transferetto.S3;

/// <summary>
/// Exposes one Amazon S3 or S3-compatible bucket prefix as a transfer endpoint.
/// </summary>
public sealed partial class S3TransferEndpoint : ITransferEndpoint, ITransferRangeEndpoint, ITransferServerSideCopyEndpoint,
    ITransferResumableWriteEndpoint, ITransferDirectoryEndpoint, IDisposable {
    private readonly IAmazonS3 _client;
    private readonly bool _ownsClient;
    private readonly string _bucketName;
    private readonly string _prefix;

    /// <summary>Initializes an endpoint and creates its AWS SDK client.</summary>
    public S3TransferEndpoint(S3EndpointOptions options) {
        if (options == null) {
            throw new ArgumentNullException(nameof(options));
        }
        Validate(options);
        _bucketName = options.BucketName;
        _prefix = NormalizePrefix(options.Prefix);
        _client = CreateClient(options);
        _ownsClient = true;
    }

    /// <summary>Initializes an endpoint with a caller-owned AWS SDK client.</summary>
    public S3TransferEndpoint(IAmazonS3 client, string bucketName, string? prefix = null) {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(bucketName)) {
            throw new ArgumentException("A bucket name is required.", nameof(bucketName));
        }
        _bucketName = bucketName;
        _prefix = NormalizePrefix(prefix);
    }

    /// <inheritdoc />
    public string Scheme => "s3";

    /// <inheritdoc />
    public string DisplayName => $"s3://{_bucketName}/{_prefix}";

    /// <inheritdoc />
    public TransferEndpointCapabilities Capabilities =>
        TransferEndpointCapabilities.Inspect |
        TransferEndpointCapabilities.List |
        TransferEndpointCapabilities.Read |
        TransferEndpointCapabilities.Write |
        TransferEndpointCapabilities.Delete |
        TransferEndpointCapabilities.Metadata |
        TransferEndpointCapabilities.Versioning |
        TransferEndpointCapabilities.ConcurrentOperations |
        TransferEndpointCapabilities.RangeRead;

    /// <inheritdoc />
    public async Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) {
        string key = ResolveKey(path);
        try {
            GetObjectMetadataResponse response = await _client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest { BucketName = _bucketName, Key = key },
                cancellationToken).ConfigureAwait(false);
            return ToItem(path, response);
        } catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound) {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransferItem>> ListAsync(
        string prefix,
        bool recursive = true,
        CancellationToken cancellationToken = default) {
        string resolvedPrefix = ResolveKey(prefix, allowEmpty: true);
        List<TransferItem> items = new();
        string? continuationToken = null;
        do {
            ListObjectsV2Response response = await _client.ListObjectsV2Async(new ListObjectsV2Request {
                BucketName = _bucketName,
                Prefix = resolvedPrefix,
                Delimiter = recursive ? null : "/",
                ContinuationToken = continuationToken
            }, cancellationToken).ConfigureAwait(false);

            items.AddRange((response.S3Objects ?? new List<S3Object>()).Select(item => new TransferItem {
                Path = ToRelativeKey(item.Key),
                Length = item.Size,
                LastModifiedUtc = item.LastModified,
                ETag = TrimETag(item.ETag)
            }));
            continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
        } while (!string.IsNullOrWhiteSpace(continuationToken));

        return items.OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
    }

    /// <inheritdoc />
    public async Task<TransferReadHandle> OpenReadAsync(
        string path,
        CancellationToken cancellationToken = default) {
        GetObjectResponse response = await _client.GetObjectAsync(new GetObjectRequest {
            BucketName = _bucketName,
            Key = ResolveKey(path)
        }, cancellationToken).ConfigureAwait(false);

        TransferItem item = new() {
            Path = path,
            Length = response.ContentLength,
            LastModifiedUtc = response.LastModified,
            ETag = TrimETag(response.ETag),
            VersionId = response.VersionId,
            ContentType = response.Headers.ContentType,
            Metadata = ReadMetadata(response.Metadata)
        };
        return new TransferReadHandle(item, new ResponseOwnedStream(response.ResponseStream, response));
    }

    /// <inheritdoc />
    public async Task<TransferReadHandle> OpenReadRangeAsync(string path, long offset, long length,
        TransferItem expectedItem, CancellationToken cancellationToken = default) {
        if (offset < 0 || length <= 0 || expectedItem?.Length is not long sourceLength
            || offset > sourceLength - length) { throw new ArgumentOutOfRangeException(nameof(offset)); }
        if (expectedItem.ETag == null && expectedItem.VersionId == null) {
            throw new NotSupportedException("S3 ranged reads require an entity tag or object version.");
        }
        GetObjectResponse response = await _client.GetObjectAsync(new GetObjectRequest {
            BucketName = _bucketName,
            Key = ResolveKey(path),
            VersionId = expectedItem.VersionId,
            EtagToMatch = expectedItem.ETag == null ? null : $"\"{expectedItem.ETag}\"",
            ByteRange = new ByteRange(offset, checked(offset + length - 1))
        }, cancellationToken).ConfigureAwait(false);
        if (response.HttpStatusCode != HttpStatusCode.PartialContent) {
            response.Dispose();
            throw new IOException("The S3 service did not honor the requested source range.");
        }
        return new TransferReadHandle(expectedItem,
            TransferContent.LimitRead(new ResponseOwnedStream(response.ResponseStream, response), length));
    }

    /// <inheritdoc />
    public async Task<TransferWriteResult> WriteAsync(
        string path,
        Stream content,
        long? length,
        TransferWriteOptions? options = null,
        CancellationToken cancellationToken = default) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        TransferWriteOptions resolvedOptions = options ?? new TransferWriteOptions();
        TransferItem? existing = null;
        if (resolvedOptions.Mode != TransferWriteMode.Overwrite) {
            existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null && resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
            if (existing != null) {
                throw new IOException($"The destination object already exists: {path}");
            }
        }

        string key = ResolveKey(path);
        Dictionary<string, string> metadata = TransferMetadata.CopyPortable(resolvedOptions.Metadata);
        S3ObjectWriteResult response;
        try {
            response = await WriteObjectAsync(
                key,
                content,
                length,
                resolvedOptions,
                metadata,
                cancellationToken).ConfigureAwait(false);
        } catch (AmazonS3Exception exception) when (
            exception.StatusCode == HttpStatusCode.PreconditionFailed &&
            resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
            existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
            throw;
        } catch (AmazonS3Exception exception) when (
            exception.StatusCode == HttpStatusCode.PreconditionFailed &&
            resolvedOptions.Mode == TransferWriteMode.FailIfExists) {
            throw new IOException($"The destination object already exists: {path}", exception);
        }
        return new TransferWriteResult(new TransferItem {
            Path = path,
            Length = response.Length,
            LastModifiedUtc = DateTimeOffset.UtcNow,
            ETag = TrimETag(response.ETag),
            VersionId = response.VersionId,
            ContentType = resolvedOptions.ContentType,
            Metadata = metadata
        }, wasWritten: true);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) {
        TransferItem? existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
        if (existing == null) {
            return false;
        }
        await _client.DeleteObjectAsync(new DeleteObjectRequest {
            BucketName = _bucketName,
            Key = ResolveKey(path)
        }, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public Task<bool> DeleteEmptyDirectoryAsync(string path, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ResolveKey(path);
        // S3 prefixes are virtual; deleting their planned child objects removes the directory.
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<TransferWriteResult?> TryCopyServerSideAsync(ITransferEndpoint source, string sourcePath,
        TransferItem sourceItem, string destinationPath, TransferWriteOptions writeOptions,
        CancellationToken cancellationToken = default) {
        if (source is not S3TransferEndpoint sourceEndpoint || !SharesService(sourceEndpoint)
            || sourceItem.Length is not long length || length < 0 || length > 5L * 1024 * 1024 * 1024
            || (sourceItem.ETag == null && sourceItem.VersionId == null)
            || !string.Equals(writeOptions.ContentType, sourceItem.ContentType, StringComparison.Ordinal)
            || writeOptions.Metadata.Any(pair => !sourceItem.Metadata.TryGetValue(pair.Key, out string? value)
                || !string.Equals(value, pair.Value, StringComparison.Ordinal))) {
            return null;
        }
        TransferItem? existing = null;
        if (writeOptions.Mode != TransferWriteMode.Overwrite) {
            existing = await GetItemAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (existing != null && writeOptions.Mode == TransferWriteMode.SkipIfExists) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
            if (existing != null) { throw new IOException($"The destination object already exists: {destinationPath}"); }
        }
        CopyObjectRequest request = new() {
            SourceBucket = sourceEndpoint._bucketName,
            SourceKey = sourceEndpoint.ResolveKey(sourcePath),
            SourceVersionId = sourceItem.VersionId,
            DestinationBucket = _bucketName,
            DestinationKey = ResolveKey(destinationPath),
            ETagToMatch = sourceItem.ETag == null ? null : $"\"{sourceItem.ETag}\"",
            IfNoneMatch = writeOptions.Mode == TransferWriteMode.Overwrite ? null : "*"
        };
        try {
            CopyObjectResponse response = await _client.CopyObjectAsync(request, cancellationToken).ConfigureAwait(false);
            return new TransferWriteResult(new TransferItem {
                Path = destinationPath,
                Length = length,
                LastModifiedUtc = DateTimeOffset.TryParse(response.LastModified,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset modified)
                    ? modified : null,
                ETag = TrimETag(response.ETag),
                VersionId = response.VersionId,
                ContentType = sourceItem.ContentType,
                Metadata = sourceItem.Metadata
            }, wasWritten: true);
        } catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.PreconditionFailed
            && writeOptions.Mode == TransferWriteMode.SkipIfExists) {
            existing = await GetItemAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (existing != null) { return new TransferWriteResult(existing, wasWritten: false); }
            throw;
        } catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.PreconditionFailed
            && writeOptions.Mode == TransferWriteMode.FailIfExists) {
            throw new IOException($"The destination object already exists: {destinationPath}", exception);
        }
    }

    private bool SharesService(S3TransferEndpoint source) {
        if (ReferenceEquals(_client, source._client)) { return true; }
        Amazon.Runtime.IClientConfig left = _client.Config;
        Amazon.Runtime.IClientConfig right = source._client.Config;
        return string.Equals(left.ServiceURL, right.ServiceURL, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.RegionEndpoint?.SystemName, right.RegionEndpoint?.SystemName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.AuthenticationRegion, right.AuthenticationRegion, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_ownsClient) {
            _client.Dispose();
        }
    }

    private static IAmazonS3 CreateClient(S3EndpointOptions options) {
        AmazonS3Config config = new() {
            ForcePathStyle = options.ForcePathStyle
        };
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl)) {
            config.ServiceURL = options.ServiceUrl;
            config.AuthenticationRegion = string.IsNullOrWhiteSpace(options.Region) ? "us-east-1" : options.Region;
        } else if (!string.IsNullOrWhiteSpace(options.Region)) {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }

        AWSCredentials? credentials = null;
        if (!string.IsNullOrWhiteSpace(options.AccessKeyId)) {
            credentials = string.IsNullOrWhiteSpace(options.SessionToken)
                ? new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey)
                : new SessionAWSCredentials(options.AccessKeyId, options.SecretAccessKey, options.SessionToken);
        }
        return credentials == null ? new AmazonS3Client(config) : new AmazonS3Client(credentials, config);
    }

    private static void Validate(S3EndpointOptions options) {
        if (string.IsNullOrWhiteSpace(options.BucketName)) {
            throw new ArgumentException("A bucket name is required.", nameof(options));
        }
        bool hasAccessKey = !string.IsNullOrWhiteSpace(options.AccessKeyId);
        bool hasSecret = !string.IsNullOrWhiteSpace(options.SecretAccessKey);
        if (hasAccessKey != hasSecret) {
            throw new ArgumentException("AccessKeyId and SecretAccessKey must be supplied together.", nameof(options));
        }
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl)) {
            if (!Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out Uri? serviceUri)) {
                throw new ArgumentException("ServiceUrl must be an absolute URI.", nameof(options));
            }
            if (serviceUri.Scheme != Uri.UriSchemeHttps && !serviceUri.IsLoopback) {
                throw new ArgumentException("Custom S3 endpoints must use HTTPS unless they are loopback test endpoints.", nameof(options));
            }
        }
    }

    private static TransferItem ToItem(string path, GetObjectMetadataResponse response) => new() {
        Path = path,
        Length = response.ContentLength,
        LastModifiedUtc = response.LastModified,
        ETag = TrimETag(response.ETag),
        VersionId = response.VersionId,
        ContentType = response.Headers.ContentType,
        Metadata = ReadMetadata(response.Metadata)
    };

    private static IReadOnlyDictionary<string, string> ReadMetadata(MetadataCollection metadata) {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (string key in metadata.Keys) {
            const string headerPrefix = "x-amz-meta-";
            string portableKey = key.StartsWith(headerPrefix, StringComparison.OrdinalIgnoreCase)
                ? key.Substring(headerPrefix.Length)
                : key;
            values[portableKey] = metadata[key];
        }
        return values;
    }

    private async Task<S3ObjectWriteResult> WriteObjectAsync(
        string key,
        Stream content,
        long? length,
        TransferWriteOptions options,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken) {
        if (S3MultipartUploader.RequiresMultipartUpload(length)) {
            S3ObjectWriteResult? multipart = await S3MultipartUploader.UploadAsync(
                _client,
                _bucketName,
                key,
                content,
                length,
                options,
                metadata,
                cancellationToken).ConfigureAwait(false);
            if (multipart != null) {
                return multipart;
            }
            length = 0;
        }

        // Validate a bounded single PUT before it can replace the destination. Larger and
        // unknown-length streams use multipart staging, validated before completion.
        using MemoryStream stagedContent = new();
        await TransferContent.CopyToAsync(content, stagedContent, length, cancellationToken).ConfigureAwait(false);
        stagedContent.Position = 0;
        PutObjectRequest request = new() {
            BucketName = _bucketName,
            Key = key,
            InputStream = stagedContent,
            AutoCloseStream = false,
            ContentType = options.ContentType,
            IfNoneMatch = options.Mode == TransferWriteMode.Overwrite ? null : "*"
        };
        foreach (KeyValuePair<string, string> pair in metadata) {
            request.Metadata[pair.Key] = pair.Value;
        }
        request.Headers.ContentLength = stagedContent.Length;
        PutObjectResponse response = await _client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return new S3ObjectWriteResult(response.ETag, response.VersionId, stagedContent.Length);
    }

    private string ResolveKey(string path, bool allowEmpty = false) {
        string normalized = (path ?? string.Empty).Replace('\\', '/').TrimStart('/');
        if (!allowEmpty && string.IsNullOrEmpty(normalized)) {
            throw new ArgumentException("An endpoint-relative object key is required.", nameof(path));
        }
        if (normalized.Split('/').Any(segment => segment == "..")) {
            throw new ArgumentException("Parent path segments are not allowed.", nameof(path));
        }
        return _prefix + normalized;
    }

    private string ToRelativeKey(string key) =>
        key.StartsWith(_prefix, StringComparison.Ordinal) ? key.Substring(_prefix.Length) : key;

    private static string NormalizePrefix(string? prefix) {
        string normalized = (prefix ?? string.Empty).Replace('\\', '/').Trim('/');
        return string.IsNullOrEmpty(normalized) ? string.Empty : normalized + "/";
    }

    private static string? TrimETag(string? eTag) => eTag?.Trim('"');

    private sealed class ResponseOwnedStream : Stream {
        private readonly Stream _inner;
        private readonly IDisposable _owner;

        internal ResponseOwnedStream(Stream inner, IDisposable owner) {
            _inner = inner;
            _owner = owner;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position {
            get => _inner.Position;
            set => _inner.Position = value;
        }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _inner.ReadAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        protected override void Dispose(bool disposing) {
            if (disposing) {
                try {
                    _inner.Dispose();
                } finally {
                    _owner.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
