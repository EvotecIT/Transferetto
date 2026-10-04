using System.Net;
using System.Net.Http;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Transferetto.AzureBlob;
using Transferetto.Core;
using Transferetto.S3;

namespace Transferetto.Tests;

public sealed class TransferettoCloudWriteSafetyTests {
    [Fact]
    public async Task EmptyS3ListingAcceptsTheSdkNullCollection() {
        using S3BoundaryClient client = new();
        using S3TransferEndpoint endpoint = new(client, "bucket");
        Assert.Empty(await endpoint.ListAsync(string.Empty));
    }

    [Theory]
    [InlineData(2L)]
    [InlineData(4L)]
    [InlineData(6L * 1024 * 1024 * 1024)]
    public async Task S3LengthMismatchNeverCommitsTheObject(long advertised) {
        using S3BoundaryClient client = new();
        using S3TransferEndpoint endpoint = new(client, "bucket");
        using MemoryStream content = new(new byte[] { 1, 2, 3 });
        await Assert.ThrowsAsync<EndOfStreamException>(() => endpoint.WriteAsync("file", content, advertised, new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }));
        Assert.False(client.Committed);
        Assert.Equal(advertised > 64L * 1024 * 1024, client.Aborted);
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task UnknownLengthS3UploadCommitsOnlyTheStreamedBytes() {
        using S3BoundaryClient client = new();
        using S3TransferEndpoint endpoint = new(client, "bucket");
        using MemoryStream content = new(new byte[] { 1, 2, 3 });
        TransferWriteResult result = await endpoint.WriteAsync("file", content, null, new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
        Assert.True(client.Committed); Assert.False(client.Aborted);
        Assert.Equal(3, result.Item.Length);
        Assert.Equal(new byte[] { 1, 2, 3 }, client.Uploaded);
    }

    [Theory]
    [InlineData(2L)]
    [InlineData(4L)]
    public async Task AzureLengthMismatchLeavesExistingBlobUncommitted(long advertised) {
        using BlobBoundaryHandler handler = new();
        using HttpClient http = new(handler);
        BlobClientOptions options = new() { Transport = new HttpClientTransport(http) };
        options.Retry.MaxRetries = 0;
        AzureBlobTransferEndpoint endpoint = new(new BlobContainerClient(new Uri("http://127.0.0.1/container"), options));
        using MemoryStream content = new(new byte[] { 1, 2, 3 });
        await Assert.ThrowsAsync<EndOfStreamException>(() => endpoint.WriteAsync("file", content, advertised, new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }));
        Assert.Equal(new byte[] { 9 }, handler.VisibleContent); Assert.Equal(0, handler.Commits);
        Assert.True(content.CanRead);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task AzureValidatedContentCommitsThroughTheSdkBlockPipeline(int length) {
        using BlobBoundaryHandler handler = new();
        using HttpClient http = new(handler);
        BlobClientOptions options = new() { Transport = new HttpClientTransport(http) };
        options.Retry.MaxRetries = 0;
        AzureBlobTransferEndpoint endpoint = new(new BlobContainerClient(new Uri("http://127.0.0.1/container"), options));
        byte[] bytes = Enumerable.Range(1, length).Select(value => (byte)value).ToArray();
        using MemoryStream content = new(bytes);
        TransferWriteResult result = await endpoint.WriteAsync("file", content, length, new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
        Assert.Equal(bytes, handler.VisibleContent); Assert.Equal(1, handler.Commits);
        Assert.Equal(length, result.Item.Length);
    }

    [Fact]
    public async Task AzureRejectsAnUnsupportedAdvertisedSizeBeforeAllocatingOrStaging() {
        using BlobBoundaryHandler handler = new();
        using HttpClient http = new(handler);
        BlobClientOptions options = new() { Transport = new HttpClientTransport(http) };
        options.Retry.MaxRetries = 0;
        AzureBlobTransferEndpoint endpoint = new(new BlobContainerClient(new Uri("http://127.0.0.1/container"), options));
        using MemoryStream content = new();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => endpoint.WriteAsync("file", content, long.MaxValue,
            new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }));
        Assert.Equal(new byte[] { 9 }, handler.VisibleContent);
        Assert.Equal(0, handler.Commits);
    }

    private sealed class S3BoundaryClient : AmazonS3Client {
        internal S3BoundaryClient() : base(new AnonymousAWSCredentials(), new AmazonS3Config { ServiceURL = "http://127.0.0.1", ForcePathStyle = true }) { }
        internal bool Committed { get; private set; }
        internal bool Aborted { get; private set; }
        internal byte[]? Uploaded { get; private set; }
        public override Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request, CancellationToken cancellationToken = default) => Task.FromResult(new ListObjectsV2Response());
        public override Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new InitiateMultipartUploadResponse { UploadId = "upload" });
        public override async Task<UploadPartResponse> UploadPartAsync(UploadPartRequest request, CancellationToken cancellationToken = default) {
            using MemoryStream bytes = new(); await request.InputStream.CopyToAsync(bytes); Uploaded = bytes.ToArray();
            return new UploadPartResponse { ETag = "part" };
        }
        public override Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default) {
            Committed = true; return Task.FromResult(new CompleteMultipartUploadResponse { ETag = "result" });
        }
        public override Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(AbortMultipartUploadRequest request, CancellationToken cancellationToken = default) {
            Aborted = true; return Task.FromResult(new AbortMultipartUploadResponse());
        }
        public override Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default) {
            Committed = true; return Task.FromResult(new PutObjectResponse { ETag = "result" });
        }
    }

    private sealed class BlobBoundaryHandler : HttpMessageHandler {
        private byte[] _staged = Array.Empty<byte>();
        internal byte[] VisibleContent { get; private set; } = new byte[] { 9 };
        internal int Commits { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri!.Query.Contains("comp=blocklist")) {
                VisibleContent = _staged; Commits++;
            } else if (request.RequestUri.Query.Contains("comp=block")) {
                _staged = await request.Content!.ReadAsByteArrayAsync();
            } else { throw new InvalidOperationException("Unexpected storage request: " + request.Method); }
            HttpResponseMessage response = new(HttpStatusCode.Created) { Content = new ByteArrayContent(Array.Empty<byte>()) };
            response.Headers.TryAddWithoutValidation("ETag", "\"result\"");
            response.Content.Headers.LastModified = DateTimeOffset.UtcNow;
            response.Headers.TryAddWithoutValidation("x-ms-request-id", "test");
            return response;
        }
    }
}
