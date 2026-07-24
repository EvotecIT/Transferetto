using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Storage.Blobs;
using Transferetto.AzureBlob;
using Transferetto.Core;
using Transferetto.S3;

namespace Transferetto.Tests;

public sealed class StorageProviderLiveTests {
    [StorageProviderLiveFact]
    public async Task S3AndAzureBlob_RoundTripAndCrossProviderCopy() {
        string suffix = Guid.NewGuid().ToString("N");
        string bucket = "transferetto-" + suffix;
        string container = "transferetto-" + suffix;
        string s3Endpoint = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ENDPOINT")!;
        string accessKey = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ACCESS_KEY") ?? "minioadmin";
        string secretKey = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SECRET_KEY") ?? "minioadmin";
        string azureConnectionString = Environment.GetEnvironmentVariable("TRANSFERETTO_AZURE_CONNECTION_STRING")!;

        AmazonS3Config s3Config = new() {
            ServiceURL = s3Endpoint,
            AuthenticationRegion = "us-east-1",
            ForcePathStyle = true
        };
        using AmazonS3Client s3Client = new(new BasicAWSCredentials(accessKey, secretKey), s3Config);
        BlobContainerClient blobClient = new(azureConnectionString, container);
        await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucket });
        await blobClient.CreateAsync();

        try {
            using S3TransferEndpoint s3 = new(s3Client, bucket, "company");
            AzureBlobTransferEndpoint blob = new(blobClient, "company");
            byte[] content = Encoding.UTF8.GetBytes("{\"schema\":\"testimo-evidence-v3\"}");
            TransferWriteOptions writeOptions = new() {
                Mode = TransferWriteMode.FailIfExists,
                ContentType = "application/json"
            };
            writeOptions.Metadata["evidence_id"] = suffix;

            TransferWriteResult uploaded = await s3.WriteAsync(
                "incoming/evidence.json",
                new MemoryStream(content),
                content.LongLength,
                writeOptions);
            Assert.True(uploaded.WasWritten);
            TransferWriteResult multipart = await s3.WriteAsync(
                "incoming/unknown-length.bin",
                new MemoryStream(content),
                null,
                writeOptions);
            Assert.True(multipart.WasWritten);
            Assert.Equal(content.LongLength, multipart.Item.Length);
            using (TransferReadHandle multipartDownload = await s3.OpenReadAsync("incoming/unknown-length.bin")) {
                using MemoryStream multipartCopy = new();
                await multipartDownload.Stream.CopyToAsync(multipartCopy);
                Assert.Equal(content, multipartCopy.ToArray());
            }

            PutObjectRequest foreignMetadataRequest = new() {
                BucketName = bucket,
                Key = "company/incoming/foreign-metadata.bin",
                InputStream = new MemoryStream(content)
            };
            foreignMetadataRequest.Metadata["build-id"] = "external";
            await s3Client.PutObjectAsync(foreignMetadataRequest);
            TransferItem? foreignMetadata = await s3.GetItemAsync("incoming/foreign-metadata.bin");
            Assert.Equal("external", foreignMetadata!.Metadata["build-id"]);
            TransferReceipt filteredMetadataReceipt = await TransferEngine.CopyAsync(
                s3,
                "incoming/foreign-metadata.bin",
                blob,
                "archive/foreign-metadata.bin");
            Assert.Equal(TransferReceiptOutcome.Copied, filteredMetadataReceipt.Outcome);
            TransferItem? filteredMetadata = await blob.GetItemAsync("archive/foreign-metadata.bin");
            Assert.DoesNotContain("build-id", filteredMetadata!.Metadata.Keys);

            TransferWriteResult skipped = await s3.WriteAsync(
                "incoming/evidence.json",
                new MemoryStream(Encoding.UTF8.GetBytes("must-not-overwrite")),
                null,
                new TransferWriteOptions { Mode = TransferWriteMode.SkipIfExists });
            Assert.False(skipped.WasWritten);
            await Assert.ThrowsAsync<IOException>(() => s3.WriteAsync(
                "incoming/evidence.json",
                new MemoryStream(Encoding.UTF8.GetBytes("must-not-overwrite")),
                null,
                new TransferWriteOptions { Mode = TransferWriteMode.FailIfExists }));

            TransferItem? inspected = await s3.GetItemAsync("incoming/evidence.json");
            Assert.NotNull(inspected);
            Assert.Equal(content.LongLength, inspected!.Length);
            Assert.Contains(await s3.ListAsync("incoming/"), item => item.Path == "incoming/evidence.json");

            TransferReceipt receipt = await TransferEngine.CopyAsync(
                s3,
                "incoming/evidence.json",
                blob,
                "archive/evidence.json");
            Assert.Equal(TransferReceiptOutcome.Copied, receipt.Outcome);
            Assert.Equal(content.LongLength, receipt.BytesTransferred);
            Assert.False(string.IsNullOrWhiteSpace(receipt.Sha256));

            using TransferReadHandle downloaded = await blob.OpenReadAsync("archive/evidence.json");
            using MemoryStream copy = new();
            await downloaded.Stream.CopyToAsync(copy);
            Assert.Equal(content, copy.ToArray());
            Assert.Equal("application/json", downloaded.Item.ContentType);
            Assert.Equal(suffix, downloaded.Item.Metadata["evidence_id"]);
            TransferWriteResult blobSkipped = await blob.WriteAsync(
                "archive/evidence.json",
                new MemoryStream(Encoding.UTF8.GetBytes("must-not-overwrite")),
                null,
                new TransferWriteOptions { Mode = TransferWriteMode.SkipIfExists });
            Assert.False(blobSkipped.WasWritten);
            await Assert.ThrowsAsync<IOException>(() => blob.WriteAsync(
                "archive/evidence.json",
                new MemoryStream(Encoding.UTF8.GetBytes("must-not-overwrite")),
                null,
                new TransferWriteOptions { Mode = TransferWriteMode.FailIfExists }));

            TransferReceipt reverseReceipt = await TransferEngine.CopyAsync(
                blob,
                "archive/evidence.json",
                s3,
                "archive/from-azure.json");
            Assert.Equal(TransferReceiptOutcome.Copied, reverseReceipt.Outcome);
            using TransferReadHandle reverseDownload = await s3.OpenReadAsync("archive/from-azure.json");
            using MemoryStream reverseCopy = new();
            await reverseDownload.Stream.CopyToAsync(reverseCopy);
            Assert.Equal(content, reverseCopy.ToArray());
            Assert.Equal(suffix, reverseDownload.Item.Metadata["evidence_id"]);

            Assert.True(await s3.DeleteAsync("incoming/evidence.json"));
            Assert.True(await s3.DeleteAsync("archive/from-azure.json"));
            Assert.True(await blob.DeleteAsync("archive/evidence.json"));
            Assert.Null(await s3.GetItemAsync("incoming/evidence.json"));
            Assert.Null(await blob.GetItemAsync("archive/evidence.json"));
        } finally {
            await blobClient.DeleteIfExistsAsync();
            ListObjectsV2Response remaining = await s3Client.ListObjectsV2Async(new ListObjectsV2Request {
                BucketName = bucket
            });
            foreach (S3Object item in remaining.S3Objects ?? new List<S3Object>()) {
                await s3Client.DeleteObjectAsync(bucket, item.Key);
            }
            await s3Client.DeleteBucketAsync(bucket);
        }
    }
}
