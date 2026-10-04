using Transferetto.Core;
using System.Runtime.InteropServices;

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

    [Fact]
    public async Task MirrorReplacesDirectoryWithFileAfterRemovingOnlyPlannedChildren() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(Path.Combine(destinationRoot, "node"));
        File.WriteAllText(Path.Combine(sourceRoot, "node"), "replacement");
        File.WriteAllText(Path.Combine(destinationRoot, "node", "old.txt"), "old");

        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(
            new FileSystemTransferEndpoint(sourceRoot), "", new FileSystemTransferEndpoint(destinationRoot), "",
            new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror });

        Assert.True(result.IsSuccess);
        Assert.Equal("replacement", File.ReadAllText(Path.Combine(destinationRoot, "node")));
        Assert.Contains(result.Items, item => item.PlanItem.Action == TransferettoSyncAction.DeleteRemoteDirectory
            && item.WasDeleted);
    }

    [Fact]
    public async Task DefaultSyncPreservesFileTimeAndSkipsAnUnchangedSecondRun() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);
        string sourcePath = Path.Combine(sourceRoot, "item.txt");
        File.WriteAllText(sourcePath, "content");
        DateTime modified = DateTime.UtcNow.AddDays(-10);
        File.SetLastWriteTimeUtc(sourcePath, modified);
        FileSystemTransferEndpoint source = new(sourceRoot);
        FileSystemTransferEndpoint destination = new(destinationRoot);

        Assert.True((await TransferettoEndpointSync.SyncAsync(source, "", destination, "")).IsSuccess);
        TransferettoEndpointSyncResult second = await TransferettoEndpointSync.SyncAsync(source, "", destination, "");

        Assert.True(second.IsSuccess);
        Assert.DoesNotContain(second.Items, item => item.Receipt != null);
        Assert.Equal(File.GetLastWriteTimeUtc(sourcePath),
            File.GetLastWriteTimeUtc(Path.Combine(destinationRoot, "item.txt")));
    }

    [Fact]
    public async Task WindowsDestinationRejectsCaseCollidingSourceBeforeWriting() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(destinationRoot);
        ListingEndpoint source = new("A.txt", "a.txt");
        FileSystemTransferEndpoint destination = new(destinationRoot);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            TransferettoEndpointSync.SyncAsync(source, "", destination, ""));
        Assert.Empty(Directory.GetFiles(destinationRoot));
    }

    [Fact]
    public async Task MirrorRejectsUnsupportedDirectoryRemovalBeforeDeletingChildren() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(Path.Combine(destinationRoot, "node"));
        File.WriteAllText(Path.Combine(sourceRoot, "node"), "new");
        string child = Path.Combine(destinationRoot, "node", "keep.txt");
        File.WriteAllText(child, "keep");

        await Assert.ThrowsAsync<NotSupportedException>(() => TransferettoEndpointSync.SyncAsync(
            new FileSystemTransferEndpoint(sourceRoot), "",
            new NoDirectoryEndpoint(new FileSystemTransferEndpoint(destinationRoot)), "",
            new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror }));
        Assert.Equal("keep", File.ReadAllText(child));
    }

    [Fact]
    public async Task MirrorWithoutDirectoryOperationsStillDeletesExtraFiles() {
        string sourceRoot = Path.Combine(_root, "source");
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(Path.Combine(destinationRoot, "old"));
        File.WriteAllText(Path.Combine(sourceRoot, "keep.txt"), "keep");
        string extra = Path.Combine(destinationRoot, "old", "child.txt");
        File.WriteAllText(extra, "extra");

        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(
            new FileSystemTransferEndpoint(sourceRoot), "",
            new NoDirectoryEndpoint(new FileSystemTransferEndpoint(destinationRoot)), "",
            new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror });

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(extra));
        Assert.True(Directory.Exists(Path.Combine(destinationRoot, "old")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(destinationRoot, "keep.txt")));
    }

    [Fact]
    public async Task ExplicitOrdinalAllowsCaseDistinctWindowsSourcePathsInPlan() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(destinationRoot);
        IReadOnlyList<TransferettoSyncPlanItem> plan = await TransferettoEndpointSync.PlanAsync(
            new ListingEndpoint("A.txt", "a.txt"), "", new FileSystemTransferEndpoint(destinationRoot), "",
            new TransferettoSyncOptions { PathComparison = TransferettoSyncPathComparison.Ordinal });
        Assert.Equal(2, plan.Count(item => item.Action == TransferettoSyncAction.UploadFile));
    }

    private sealed class NoDirectoryEndpoint : ITransferEndpoint {
        private readonly ITransferEndpoint _inner;
        internal NoDirectoryEndpoint(ITransferEndpoint inner) { _inner = inner; }
        public string Scheme => "test";
        public string DisplayName => _inner.DisplayName;
        public TransferEndpointCapabilities Capabilities => _inner.Capabilities;
        public Task<IReadOnlyList<TransferItem>> ListAsync(string prefix, bool recursive = true,
            CancellationToken cancellationToken = default) => _inner.ListAsync(prefix, recursive, cancellationToken);
        public Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) =>
            _inner.GetItemAsync(path, cancellationToken);
        public Task<TransferReadHandle> OpenReadAsync(string path, CancellationToken cancellationToken = default) =>
            _inner.OpenReadAsync(path, cancellationToken);
        public Task<TransferWriteResult> WriteAsync(string path, Stream content, long? length,
            TransferWriteOptions? options = null, CancellationToken cancellationToken = default) =>
            _inner.WriteAsync(path, content, length, options, cancellationToken);
        public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) =>
            _inner.DeleteAsync(path, cancellationToken);
    }

    private sealed class ListingEndpoint : ITransferEndpoint {
        private readonly TransferItem[] _items;
        internal ListingEndpoint(params string[] paths) {
            _items = paths.Select(path => new TransferItem { Path = path, Length = 1 }).ToArray();
        }
        public string Scheme => "s3";
        public string DisplayName => "s3://test/";
        public TransferEndpointCapabilities Capabilities => TransferEndpointCapabilities.List |
            TransferEndpointCapabilities.Read | TransferEndpointCapabilities.Inspect;
        public Task<IReadOnlyList<TransferItem>> ListAsync(string prefix, bool recursive = true,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TransferItem>>(_items);
        public Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<TransferReadHandle> OpenReadAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<TransferWriteResult> WriteAsync(string path, Stream content, long? length,
            TransferWriteOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    public void Dispose() {
        if (Directory.Exists(_root)) { Directory.Delete(_root, recursive: true); }
    }
}
