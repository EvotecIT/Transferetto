using System.Security.Cryptography;
using System.Text;
using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoCoreEndpointTests : IDisposable {
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "Transferetto.Tests",
        Guid.NewGuid().ToString("N"));

    public TransferettoCoreEndpointTests() {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task CopyAsync_StreamsContentAndReturnsPortableReceipt() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        byte[] content = Encoding.UTF8.GetBytes("transferetto-core");
        File.WriteAllBytes(Path.Combine(sourceRoot, "evidence.json"), content);

        TransferReceipt receipt = await TransferEngine.CopyAsync(
            new FileSystemTransferEndpoint(sourceRoot),
            "evidence.json",
            new FileSystemTransferEndpoint(destinationRoot),
            "archive/evidence.json");

        Assert.Equal(TransferReceiptOutcome.Copied, receipt.Outcome);
        Assert.Equal(content.LongLength, receipt.BytesTransferred);
        Assert.Equal(CalculateSha256(content), receipt.Sha256);
        Assert.Equal(content, File.ReadAllBytes(Path.Combine(destinationRoot, "archive", "evidence.json")));
        Assert.NotEqual(Guid.Empty, receipt.CorrelationId);
        Assert.True(receipt.CompletedAtUtc >= receipt.StartedAtUtc);
    }

    [Fact]
    public async Task CopyAsync_SkipIfExistsDoesNotConsumeOrMisreportContent() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "source.txt"), "new");
        File.WriteAllText(Path.Combine(destinationRoot, "target.txt"), "existing");

        TransferReceipt receipt = await TransferEngine.CopyAsync(
            new FileSystemTransferEndpoint(sourceRoot),
            "source.txt",
            new FileSystemTransferEndpoint(destinationRoot),
            "target.txt",
            new TransferCopyOptions {
                WriteOptions = new TransferWriteOptions { Mode = TransferWriteMode.SkipIfExists }
            });

        Assert.Equal(TransferReceiptOutcome.Skipped, receipt.Outcome);
        Assert.Equal(0, receipt.BytesTransferred);
        Assert.Null(receipt.Sha256);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(destinationRoot, "target.txt")));
    }

    [Fact]
    public async Task FileSystemEndpoint_RejectsRootEscapeAcrossOperations() {
        FileSystemTransferEndpoint endpoint = new(_root);

        await Assert.ThrowsAsync<ArgumentException>(() => endpoint.GetItemAsync("../outside.txt"));
        await Assert.ThrowsAsync<ArgumentException>(() => endpoint.OpenReadAsync("../outside.txt"));
        await Assert.ThrowsAsync<ArgumentException>(() => endpoint.WriteAsync(
            "../outside.txt",
            new MemoryStream(new byte[] { 1 }),
            1));
        await Assert.ThrowsAsync<ArgumentException>(() => endpoint.DeleteAsync("../outside.txt"));
    }

    [Fact]
    public async Task FileSystemEndpoint_EnforcesAllCollisionModes() {
        FileSystemTransferEndpoint endpoint = new(_root);
        await endpoint.WriteAsync(
            "item.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("one")),
            3,
            new TransferWriteOptions { Mode = TransferWriteMode.FailIfExists });

        await Assert.ThrowsAsync<IOException>(() => endpoint.WriteAsync(
            "item.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("two")),
            3,
            new TransferWriteOptions { Mode = TransferWriteMode.FailIfExists }));

        TransferWriteResult skipped = await endpoint.WriteAsync(
            "item.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("two")),
            3,
            new TransferWriteOptions { Mode = TransferWriteMode.SkipIfExists });
        Assert.False(skipped.WasWritten);

        TransferWriteResult overwritten = await endpoint.WriteAsync(
            "item.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("two")),
            3,
            new TransferWriteOptions { Mode = TransferWriteMode.Overwrite });
        Assert.True(overwritten.WasWritten);
        Assert.Equal("two", File.ReadAllText(Path.Combine(_root, "item.txt")));
    }

    [Fact]
    public void FileSystemEndpoint_PreservesFilesystemVolumeRoots() {
        string volumeRoot = Path.GetPathRoot(Path.GetFullPath(_root))!;
        FileSystemTransferEndpoint endpoint = new(volumeRoot);

        Assert.Equal(new Uri(volumeRoot).AbsoluteUri, endpoint.DisplayName);
        Assert.False(endpoint.Capabilities.HasFlag(TransferEndpointCapabilities.Metadata));
    }

    public void Dispose() {
        if (Directory.Exists(_root)) {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static string CalculateSha256(byte[] content) {
        using SHA256 sha256 = SHA256.Create();
        return BitConverter.ToString(sha256.ComputeHash(content)).Replace("-", string.Empty).ToLowerInvariant();
    }
}
