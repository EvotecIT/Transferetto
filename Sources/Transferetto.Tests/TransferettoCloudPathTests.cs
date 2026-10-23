using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Transferetto.AzureBlob;
using Transferetto.Core;
using Transferetto.S3;

namespace Transferetto.Tests;

public sealed class TransferettoCloudPathTests {
    [Theory]
    [InlineData(@"reports\latest.txt", "reports/latest.txt")]
    [InlineData("/latest.txt", "latest.txt")]
    public async Task S3ListedKeysAddressTheSameObjectForReadWriteAndDelete(string name, string alias) {
        using MemoryS3 client = new();
        const string prefix = "/scope\\raw/";
        client.Files[prefix + name] = Encoding.UTF8.GetBytes("selected");
        client.Files[prefix + alias] = Encoding.UTF8.GetBytes("preserve");
        using S3TransferEndpoint endpoint = new(client, "bucket", prefix);
        TransferItem listed = Assert.Single(await endpoint.ListAsync(""), item => item.Path == name);
        using (TransferReadHandle read = await endpoint.OpenReadAsync(listed.Path)) {
            using StreamReader text = new(read.Stream);
            Assert.Equal("selected", await text.ReadToEndAsync());
        }
        using MemoryStream replacement = new(Encoding.UTF8.GetBytes("replacement"));
        await endpoint.WriteAsync(listed.Path, replacement, replacement.Length,
            new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
        Assert.Equal("replacement", Encoding.UTF8.GetString(client.Files[prefix + name]));
        Assert.True(await endpoint.DeleteAsync(listed.Path));
        Assert.False(client.Files.ContainsKey(prefix + name));
        Assert.Equal("preserve", Encoding.UTF8.GetString(client.Files[prefix + alias]));
        Assert.NotEqual(endpoint.GetPathIdentity(name), endpoint.GetPathIdentity(alias));
    }

    [Theory]
    [InlineData(@"reports\latest.txt", "reports/latest.txt", "scope\\raw/")]
    [InlineData("/latest.txt", "latest.txt", "")]
    [InlineData("/latest.txt", "latest.txt", "scope/")]
    public async Task AzureListedNamesAddressTheSameBlobOrFailBeforeSubstitutingAnotherName(string name, string alias, string prefix) {
        using MemoryBlobHandler handler = new();
        handler.Names.Add(prefix + name);
        handler.Names.Add(prefix + alias);
        using HttpClient http = new(handler);
        BlobClientOptions options = new() { Transport = new HttpClientTransport(http) };
        options.Retry.MaxRetries = 0;
        AzureBlobTransferEndpoint endpoint = new(new BlobContainerClient(new Uri("https://account.blob.core.windows.net/container"), options), prefix);
        TransferItem listed = Assert.Single(await endpoint.ListAsync(""), item => item.Path == name);
        if (prefix.Length == 0 && name.StartsWith("/", StringComparison.Ordinal)) {
            await Assert.ThrowsAsync<ArgumentException>(() => endpoint.GetItemAsync(listed.Path));
            await Assert.ThrowsAsync<ArgumentException>(() => endpoint.DeleteAsync(listed.Path));
            using MemoryStream unsupported = new(new byte[] { 1 });
            await Assert.ThrowsAsync<ArgumentException>(() => endpoint.WriteAsync(listed.Path, unsupported, 1,
                new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }));
            Assert.Null(handler.LastInspected);
            Assert.Null(handler.LastWritten);
            Assert.Contains(name, handler.Names);
            Assert.Contains(alias, handler.Names);
            return;
        }
        Assert.NotNull(await endpoint.GetItemAsync(listed.Path));
        Assert.Equal(prefix + name, handler.LastInspected);
        using MemoryStream replacement = new(Encoding.UTF8.GetBytes("replacement"));
        await endpoint.WriteAsync(listed.Path, replacement, replacement.Length,
            new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
        Assert.Equal(prefix + name, handler.LastWritten);
        Assert.True(await endpoint.DeleteAsync(listed.Path));
        Assert.DoesNotContain(prefix + name, handler.Names);
        Assert.Contains(prefix + alias, handler.Names);
        Assert.NotEqual(endpoint.GetPathIdentity(name), endpoint.GetPathIdentity(alias));
    }

    private sealed class MemoryS3 : AmazonS3Client {
        internal Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);
        internal MemoryS3() : base(new AnonymousAWSCredentials(), new AmazonS3Config {
            ServiceURL = "http://127.0.0.1", ForcePathStyle = true
        }) { }
        public override Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request,
            CancellationToken cancellationToken = default) => Task.FromResult(new ListObjectsV2Response {
                S3Objects = Files.Where(file => file.Key.StartsWith(request.Prefix, StringComparison.Ordinal))
                    .Select(file => new S3Object { Key = file.Key, Size = file.Value.Length }).ToList()
            });
        public override Task<GetObjectMetadataResponse> GetObjectMetadataAsync(GetObjectMetadataRequest request,
            CancellationToken cancellationToken = default) => Task.FromResult(new GetObjectMetadataResponse {
                ContentLength = Files[request.Key].Length, ETag = "fixture"
            });
        public override Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request,
            CancellationToken cancellationToken = default) => Task.FromResult(new GetObjectResponse {
                ResponseStream = new MemoryStream(Files[request.Key], writable: false),
                ContentLength = Files[request.Key].Length, ETag = "fixture"
            });
        public override async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request,
            CancellationToken cancellationToken = default) {
            using MemoryStream bytes = new();
            await request.InputStream.CopyToAsync(bytes);
            Files[request.Key] = bytes.ToArray();
            return new PutObjectResponse { ETag = "replacement" };
        }
        public override Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request,
            CancellationToken cancellationToken = default) {
            Assert.True(Files.Remove(request.Key));
            return Task.FromResult(new DeleteObjectResponse());
        }
    }

    // The SDK's HTTP boundary is replaced entirely; no request reaches a socket or a local listener.
    private sealed class MemoryBlobHandler : HttpMessageHandler {
        internal HashSet<string> Names { get; } = new(StringComparer.Ordinal);
        internal string? LastInspected { get; private set; }
        internal string? LastWritten { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            HttpResponseMessage response;
            if (request.RequestUri!.Query.Contains("comp=list")) {
                XElement xml = new("EnumerationResults",
                    new XAttribute("ServiceEndpoint", "https://account.blob.core.windows.net/"), new XAttribute("ContainerName", "container"),
                    new XElement("Blobs", Names.Select(name => new XElement("Blob", new XElement("Name", name),
                        new XElement("Properties", new XElement("Content-Length", "8"), new XElement("BlobType", "BlockBlob"),
                            new XElement("Etag", "\"fixture\""), new XElement("Last-Modified", "Thu, 01 Jan 2026 00:00:00 GMT"))))),
                    new XElement("NextMarker"));
                response = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(xml.ToString(), Encoding.UTF8, "application/xml")
                };
            } else {
                Assert.StartsWith("/container/", request.RequestUri.AbsolutePath, StringComparison.Ordinal);
                string name = Uri.UnescapeDataString(request.RequestUri.AbsolutePath.Substring("/container/".Length));
                Assert.Contains(name, Names);
                if (request.Method == HttpMethod.Delete) {
                    Names.Remove(name);
                    response = new HttpResponseMessage(HttpStatusCode.Accepted);
                } else if (request.Method == HttpMethod.Put) {
                    LastWritten = name;
                    response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new ByteArrayContent(Array.Empty<byte>()) };
                    response.Headers.TryAddWithoutValidation("ETag", "\"replacement\"");
                    response.Content.Headers.LastModified = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
                } else {
                    Assert.Equal(HttpMethod.Head, request.Method);
                    LastInspected = name;
                    response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[8]) };
                    response.Headers.TryAddWithoutValidation("ETag", "\"fixture\"");
                    response.Headers.TryAddWithoutValidation("x-ms-blob-type", "BlockBlob");
                    response.Content.Headers.LastModified = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
                }
            }
            response.Headers.TryAddWithoutValidation("x-ms-request-id", "fixture");
            return Task.FromResult(response);
        }
    }
}
