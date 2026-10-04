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
    public async Task ProviderResumeReusesStagedPartsAfterCancellation() {
        string suffix = Guid.NewGuid().ToString("N");
        string bucket = "transferetto-" + suffix;
        string container = "transferetto-" + suffix;
        string root = Path.Combine(Path.GetTempPath(), "transferetto-resume-" + suffix);
        Directory.CreateDirectory(root);
        byte[] payload = new byte[11 * 1024 * 1024];
        new Random(17).NextBytes(payload);
        File.WriteAllBytes(Path.Combine(root, "source.bin"), payload);
        AmazonS3Config config = new() {
            ServiceURL = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ENDPOINT")!,
            AuthenticationRegion = "us-east-1", ForcePathStyle = true,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };
        using AmazonS3Client client = new(new BasicAWSCredentials(
            Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ACCESS_KEY") ?? "minioadmin",
            Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SECRET_KEY") ?? "minioadmin"), config);
        BlobContainerClient blobClient = new(Environment.GetEnvironmentVariable("TRANSFERETTO_AZURE_CONNECTION_STRING")!, container);
        await client.PutBucketAsync(new PutBucketRequest { BucketName = bucket });
        await blobClient.CreateAsync();
        try {
            FileSystemTransferEndpoint source = new(root);
            using S3TransferEndpoint s3 = new(client, bucket);
            AzureBlobTransferEndpoint azure = new(blobClient);
            foreach (ITransferEndpoint destination in new ITransferEndpoint[] { s3, azure }) {
                string checkpoint = Path.Combine(root, destination.Scheme + ".checkpoint.json");
                using CancellationTokenSource cancel = new();
                TransferResumeOptions options = new() {
                    CheckpointPath = checkpoint, ChunkBytes = 5 * 1024 * 1024,
                    CopyOptions = new TransferCopyOptions { VerifyDestination = true,
                        Progress = new InlineProgress<TransferProgress>(progress => {
                            if (progress.BytesTransferred >= 5 * 1024 * 1024) { cancel.Cancel(); }
                        }) }
                };
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => TransferEngine.CopyResumableAsync(
                    source, "source.bin", destination, "target.bin", options, cancel.Token));
                Assert.True(File.Exists(checkpoint));
                Assert.Null(await destination.GetItemAsync("target.bin"));
                string checkpointJson = File.ReadAllText(checkpoint);
                System.Text.RegularExpressions.Match stagedSession = System.Text.RegularExpressions.Regex.Match(
                    checkpointJson, "\\\"SessionId\\\":\\\"([^\\\"]+)\\\"");
                Assert.True(stagedSession.Success);
                Assert.Single(await ((ITransferResumableWriteEndpoint)destination).ListResumablePartsAsync(
                    "target.bin", stagedSession.Groups[1].Value));
                options.CopyOptions.Progress = null;
                TransferReceipt receipt = await TransferEngine.CopyResumableAsync(source, "source.bin",
                    destination, "target.bin", options);
                Assert.Equal(TransferReceiptOutcome.Copied, receipt.Outcome);
                Assert.True(receipt.DestinationVerified);
                Assert.Equal(payload.LongLength, receipt.BytesTransferred);
                Assert.False(File.Exists(checkpoint));
                using TransferReadHandle read = await destination.OpenReadAsync("target.bin");
                using MemoryStream copied = new();
                await read.Stream.CopyToAsync(copied);
                Assert.Equal(payload, copied.ToArray());
            }
        } finally {
            string s3Checkpoint = Path.Combine(root, "s3.checkpoint.json");
            if (File.Exists(s3Checkpoint)) {
                string json = File.ReadAllText(s3Checkpoint);
                System.Text.RegularExpressions.Match session = System.Text.RegularExpressions.Regex.Match(json,
                    "\\\"SessionId\\\":\\\"([^\\\"]+)\\\"");
                if (session.Success) {
                    await client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest {
                        BucketName = bucket, Key = "target.bin", UploadId = session.Groups[1].Value
                    });
                }
            }
            ListObjectsV2Response objects = await client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = bucket });
            foreach (S3Object item in objects.S3Objects ?? new List<S3Object>()) {
                await client.DeleteObjectAsync(bucket, item.Key);
            }
            await client.DeleteBucketAsync(new DeleteBucketRequest { BucketName = bucket });
            await blobClient.DeleteIfExistsAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class InlineProgress<T> : IProgress<T> {
        private readonly Action<T> _report;
        internal InlineProgress(Action<T> report) { _report = report; }
        public void Report(T value) => _report(value);
    }

    [StorageProviderLiveFact]
    public async Task ProviderSideCopyUsesCloudServiceAndPreservesCopyModes() {
        string suffix = Guid.NewGuid().ToString("N");
        string bucket = "transferetto-" + suffix;
        string container = "transferetto-" + suffix;
        AmazonS3Config config = new() {
            ServiceURL = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ENDPOINT")!,
            AuthenticationRegion = "us-east-1", ForcePathStyle = true,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };
        BasicAWSCredentials credentials = new(
            Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ACCESS_KEY") ?? "minioadmin",
            Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SECRET_KEY") ?? "minioadmin");
        using AmazonS3Client client = new(credentials, config);
        using AmazonS3Client destinationClient = new(credentials, new AmazonS3Config {
            ServiceURL = config.ServiceURL, AuthenticationRegion = "us-east-1", ForcePathStyle = true,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        });
        BlobContainerClient blobClient = new(Environment.GetEnvironmentVariable("TRANSFERETTO_AZURE_CONNECTION_STRING")!, container);
        await client.PutBucketAsync(new PutBucketRequest { BucketName = bucket });
        await blobClient.CreateAsync();
        try {
            using S3TransferEndpoint s3Source = new(client, bucket, "source");
            using S3TransferEndpoint s3Destination = new(destinationClient, bucket, "destination");
            AzureBlobTransferEndpoint azureSource = new(blobClient, "source");
            AzureBlobTransferEndpoint azureDestination = new(blobClient, "destination");
            byte[] payload = Encoding.UTF8.GetBytes("provider-side-copy");
            foreach ((ITransferEndpoint source, ITransferEndpoint destination) in new[] {
                ((ITransferEndpoint)s3Source, (ITransferEndpoint)s3Destination),
                ((ITransferEndpoint)azureSource, (ITransferEndpoint)azureDestination)
            }) {
                await source.WriteAsync("input.txt", new MemoryStream(payload), payload.Length,
                    new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
                TransferItem inspected = (await source.GetItemAsync("input.txt"))!;
                using (TransferReadHandle range = await ((ITransferRangeEndpoint)source).OpenReadRangeAsync(
                    "input.txt", 2, 5, inspected)) {
                    using MemoryStream rangedContent = new();
                    await range.Stream.CopyToAsync(rangedContent);
                    Assert.Equal(payload.Skip(2).Take(5).ToArray(), rangedContent.ToArray());
                }
                TransferReceipt native = await TransferEngine.CopyAsync(source, "input.txt", destination, "output.txt",
                    new TransferCopyOptions { PreferServerSideCopy = true });
                Assert.True(native.ServerSideCopy);
                Assert.Null(native.Sha256);
                Assert.Equal(payload.Length, native.BytesTransferred);
                using TransferReadHandle read = await destination.OpenReadAsync("output.txt");
                using MemoryStream copied = new();
                await read.Stream.CopyToAsync(copied);
                Assert.Equal(payload, copied.ToArray());
                TransferReceipt skipped = await TransferEngine.CopyAsync(source, "input.txt", destination, "output.txt",
                    new TransferCopyOptions { PreferServerSideCopy = true,
                        WriteOptions = new TransferWriteOptions { Mode = TransferWriteMode.SkipIfExists } });
                Assert.Equal(TransferReceiptOutcome.Skipped, skipped.Outcome);
                Assert.False(skipped.ServerSideCopy);
            }
        } finally {
            ListObjectsV2Response objects = await client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = bucket });
            foreach (S3Object item in objects.S3Objects ?? new List<S3Object>()) {
                await client.DeleteObjectAsync(bucket, item.Key);
            }
            await client.DeleteBucketAsync(new DeleteBucketRequest { BucketName = bucket });
            await blobClient.DeleteIfExistsAsync();
        }
    }

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
