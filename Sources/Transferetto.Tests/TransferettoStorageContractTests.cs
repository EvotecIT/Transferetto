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
}
