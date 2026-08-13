using System.Reflection;
using Amazon.S3.Model;
using Azure;
using Azure.Storage.Blobs.Models;
using Transferetto.AzureBlob;
using Transferetto.Core;
using Transferetto.S3;

namespace Transferetto.Tests;

public sealed class TransferettoStorageContractTests {
    [Fact]
    public void S3Endpoint_RejectsIncompleteOrInsecureExplicitConfiguration() {
        Assert.Throws<ArgumentException>(() => new S3TransferEndpoint(new S3EndpointOptions {
            BucketName = "evidence",
            AccessKeyId = "key"
        }));
        Assert.Throws<ArgumentException>(() => new S3TransferEndpoint(new S3EndpointOptions {
            BucketName = "evidence",
            ServiceUrl = "http://storage.example.com",
            AccessKeyId = "key",
            SecretAccessKey = "secret"
        }));
    }

    [Fact]
    public void AzureBlobEndpoint_DoesNotCreateOrAdministerContainers() {
        AzureBlobTransferEndpoint endpoint = new(
            new Uri("https://account.blob.core.windows.net/evidence?sv=test"),
            "company-a");

        Assert.Equal("azureblob", endpoint.Scheme);
        Assert.DoesNotContain("sv=test", endpoint.DisplayName, StringComparison.Ordinal);
        Assert.True(endpoint.Capabilities.HasFlag(TransferEndpointCapabilities.Write));
    }

    [Fact]
    public void AzureBlobEndpoint_RejectsInsecureRemoteContainerUri() {
        Assert.Throws<ArgumentException>(() => new AzureBlobTransferEndpoint(
            new Uri("http://storage.example.com/evidence?sig=secret")));
        Assert.Throws<ArgumentException>(() => new AzureBlobTransferEndpoint(
            "DefaultEndpointsProtocol=http;AccountName=test;AccountKey=YWJjZA==;" +
            "BlobEndpoint=http://storage.example.com/test;",
            "evidence"));
        Assert.Throws<ArgumentException>(() => new AzureBlobTransferEndpoint(
            new Azure.Storage.Blobs.BlobContainerClient(
                new Uri("http://storage.example.com/evidence"))));
    }

    [Fact]
    public void AzureBlobEndpoint_RejectsUserInfoThatCouldLeakThroughDisplayName() {
        Assert.Throws<ArgumentException>(() => new AzureBlobTransferEndpoint(
            new Uri("https://user:secret@storage.example.com/evidence")));
        Assert.Throws<ArgumentException>(() => new AzureBlobTransferEndpoint(
            new Azure.Storage.Blobs.BlobContainerClient(
                new Uri("https://user:secret@storage.example.com/evidence"))));
    }

    [Fact]
    public void AzureBlobEndpoint_BindsOpenReadToInspectedETag() {
        MethodInfo method = typeof(AzureBlobTransferEndpoint).GetMethod(
            "CreateOpenReadOptions",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        BlobOpenReadOptions options = (BlobOpenReadOptions)method.Invoke(
            null,
            new object[] { new ETag("\"source-version\"") })!;

        Assert.Equal(new ETag("\"source-version\""), options.Conditions!.IfMatch);
    }

    [Fact]
    public void ObjectStorageEndpoints_PreserveWhitespaceOnlyKeys() {
        using S3TransferEndpoint s3 = new(new S3EndpointOptions {
            BucketName = "evidence",
            ServiceUrl = "http://127.0.0.1:9000"
        });
        AzureBlobTransferEndpoint blob = new(
            new Uri("https://account.blob.core.windows.net/evidence"));

        Assert.Equal(" ", InvokePathResolver(s3, "ResolveKey", " "));
        Assert.Equal(" ", InvokePathResolver(blob, "ResolveName", " "));
    }

    [Fact]
    public void S3Endpoint_PreservesProviderMetadataOnRead() {
        MetadataCollection metadata = new();
        metadata["x-amz-meta-build-id"] = "external";
        MethodInfo method = typeof(S3TransferEndpoint).GetMethod(
            "ReadMetadata",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        IReadOnlyDictionary<string, string> result =
            (IReadOnlyDictionary<string, string>)method.Invoke(null, new object[] { metadata })!;

        Assert.Equal("external", result["build-id"]);
    }

    [Fact]
    public void S3Endpoint_SelectsMultipartForUnknownAndOversizedContent() {
        Type uploader = typeof(S3TransferEndpoint).Assembly.GetType(
            "Transferetto.S3.S3MultipartUploader",
            throwOnError: true)!;
        MethodInfo method = uploader.GetMethod(
            "RequiresMultipartUpload",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        const long maximumSinglePut = 5L * 1024 * 1024 * 1024;

        Assert.True((bool)method.Invoke(null, new object?[] { null })!);
        Assert.False((bool)method.Invoke(null, new object?[] { maximumSinglePut })!);
        Assert.True((bool)method.Invoke(null, new object?[] { maximumSinglePut + 1 })!);
    }

    [Theory]
    [InlineData("evidence-id")]
    [InlineData("1evidence")]
    [InlineData("évidence")]
    [InlineData("")]
    public void PortableMetadata_RejectsProviderSpecificNames(string name) {
        Assert.Throws<ArgumentException>(() => TransferMetadata.ValidateName(name));
    }

    [Theory]
    [InlineData("evidence_id")]
    [InlineData("_schema")]
    [InlineData("Company123")]
    public void PortableMetadata_AcceptsCrossProviderNames(string name) {
        TransferMetadata.ValidateName(name);
    }

    private static string InvokePathResolver(object endpoint, string methodName, string path) {
        MethodInfo method = endpoint.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (string)method.Invoke(endpoint, new object[] { path, false })!;
    }
}
