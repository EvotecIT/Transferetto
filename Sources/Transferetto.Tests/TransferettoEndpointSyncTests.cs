using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoEndpointSyncTests : IDisposable {
    private readonly string _root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task MirrorUsesExistingPlannerForCopyAndScopedDeletes() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(Path.Combine(sourceRoot, "site", "nested"));
        Directory.CreateDirectory(Path.Combine(destinationRoot, "site"));
        File.WriteAllText(Path.Combine(sourceRoot, "site", "nested", "keep.txt"), "new");
        File.WriteAllText(Path.Combine(destinationRoot, "site", "old.txt"), "old");
        File.WriteAllText(Path.Combine(destinationRoot, "site-other.txt"), "outside");
        FileSystemTransferEndpoint source = new(sourceRoot);
        FileSystemTransferEndpoint destination = new(destinationRoot);

        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(source, "site", destination, "site",
            new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror, Comparison = TransferettoSyncComparison.Size });

        Assert.True(result.IsSuccess);
        Assert.Equal("new", File.ReadAllText(Path.Combine(destinationRoot, "site", "nested", "keep.txt")));
        Assert.False(File.Exists(Path.Combine(destinationRoot, "site", "old.txt")));
        Assert.Equal("outside", File.ReadAllText(Path.Combine(destinationRoot, "site-other.txt")));
        Assert.Contains(result.Items, item => item.Receipt?.Outcome == TransferReceiptOutcome.Copied);
        Assert.Contains(result.Items, item => item.WasDeleted);
    }

    [Fact]
    public async Task DryRunPlansChangesWithoutWriting() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "entry.txt"), "content");

        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(
            new FileSystemTransferEndpoint(sourceRoot), "", new FileSystemTransferEndpoint(destinationRoot), "",
            new TransferettoSyncOptions { DryRun = true });

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Plan, item => item.Action == TransferettoSyncAction.UploadFile);
        Assert.False(File.Exists(Path.Combine(destinationRoot, "entry.txt")));
    }

    [Fact]
    public async Task EmptySourceMirrorNeedsExplicitOptIn() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);
        string path = Path.Combine(destinationRoot, "preserve.txt");
        File.WriteAllText(path, "preserve");
        FileSystemTransferEndpoint source = new(sourceRoot);
        FileSystemTransferEndpoint destination = new(destinationRoot);
        TransferettoSyncOptions options = new() { Mode = TransferettoSyncMode.Mirror };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransferettoEndpointSync.SyncAsync(source, "", destination, "", options));
        Assert.Equal("preserve", File.ReadAllText(path));
        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(
            source, "", destination, "", options, allowEmptySourceMirror: true);
        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(path));
    }

    public void Dispose() {
        if (Directory.Exists(_root)) { Directory.Delete(_root, recursive: true); }
    }
}
