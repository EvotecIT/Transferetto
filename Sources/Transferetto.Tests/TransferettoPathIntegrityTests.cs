using System.Runtime.InteropServices;
using System.Text;
using FluentFTP;
using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoPathIntegrityTests : IDisposable {
    private readonly string _root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));

    public TransferettoPathIntegrityTests() { Directory.CreateDirectory(_root); }

    [Fact]
    public void EmptyDirectoryIdentityAgreesWithItsFilesystemAndRemainsStableAfterWriting() {
        string root = Path.Combine(_root, "empty");
        Directory.CreateDirectory(root);
        string first = Path.Combine(root, "a.txt");
        string second = Path.Combine(root, "A.txt");
        string initialIdentity = TransferFileSystem.GetPathIdentity(first);
        TransferPathRegistry registry = new(root);
        registry.Add("a.txt");
        File.WriteAllText(first, "preserve");
        bool insensitive = File.Exists(second);
        Assert.Equal(initialIdentity, TransferFileSystem.GetPathIdentity(first));
        if (insensitive) {
            Assert.Equal(initialIdentity, TransferFileSystem.GetPathIdentity(second));
            Assert.Throws<InvalidDataException>(() => registry.Add("A.txt"));
        } else {
            Assert.NotEqual(initialIdentity, TransferFileSystem.GetPathIdentity(second));
            registry.Add("A.txt");
        }
        Assert.Equal("preserve", File.ReadAllText(first));
    }

    [Fact]
    public async Task ScpCaseCollisionsLeaveNoDirectoryAndDistinctNamesRoundTrip() {
        bool insensitive = IsCaseInsensitive();
        string selected = Path.Combine(_root, "selected");
        using MemoryStream records = new(Encoding.UTF8.GetBytes(
            "D0755 0 root\nC0644 3 a.txt\none\0C0644 3 A.txt\ntwo\0E\n"));
        using MemoryStream acks = new();
        ScpDirectoryReceiver receiver = new(records, acks, selected);

        if (insensitive) {
            await Assert.ThrowsAsync<InvalidDataException>(() => receiver.ReceiveAsync(CancellationToken.None));
            Assert.False(Directory.Exists(selected));
        } else {
            await receiver.ReceiveAsync(CancellationToken.None);
            Assert.Equal("one", File.ReadAllText(Path.Combine(selected, "a.txt")));
            Assert.Equal("two", File.ReadAllText(Path.Combine(selected, "A.txt")));
            Assert.Equal(2, Directory.GetFiles(selected).Length);
        }
    }

    [Fact]
    public void RemoteDirectoryAliasesAreRejectedBeforeTheirChildrenCanMerge() {
        bool insensitive = IsCaseInsensitive();
        TransferPathRegistry registry = new(_root);
        registry.Add("Reports/one.txt");
        if (insensitive) { Assert.Throws<InvalidDataException>(() => registry.Add("reports/two.txt")); }
        else { Assert.Equal(Path.Combine(_root, "reports", "two.txt"), registry.Add("reports/two.txt")); }
    }

    [Fact]
    public async Task EndpointSyncRejectsCaseEquivalentParentDirectoriesBeforeWriting() {
        bool insensitive = IsCaseInsensitive();
        string destinationRoot = Path.Combine(_root, "parents");
        Directory.CreateDirectory(destinationRoot);
        ExactEndpoint source = new(("Reports/one.txt", "one"), ("reports/two.txt", "two"));
        Task<TransferettoEndpointSyncResult> Synchronize() => TransferettoEndpointSync.SyncAsync(source, "",
            new FileSystemTransferEndpoint(destinationRoot), "");
        if (insensitive) {
            await Assert.ThrowsAsync<InvalidDataException>(Synchronize);
            Assert.Empty(Directory.GetFileSystemEntries(destinationRoot));
            Assert.Empty(source.Reads);
        } else {
            Assert.True((await Synchronize()).IsSuccess);
            Assert.Equal("one", File.ReadAllText(Path.Combine(destinationRoot, "Reports", "one.txt")));
            Assert.Equal("two", File.ReadAllText(Path.Combine(destinationRoot, "reports", "two.txt")));
        }
    }

    [Fact]
    public async Task EndpointSyncNeverSubstitutesANestedFileForALiteralBackslashName() {
        string destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(Path.Combine(destinationRoot, "literal"));
        File.WriteAllText(Path.Combine(destinationRoot, "literal", "name.txt"), "garbage");
        ExactEndpoint source = new((@"literal\name.txt", "payload"));
        FileSystemTransferEndpoint destination = new(destinationRoot);
        TransferettoSyncOptions options = new() { Comparison = TransferettoSyncComparison.Size };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            await Assert.ThrowsAsync<ArgumentException>(() => TransferettoEndpointSync.SyncAsync(source, "", destination, "", options));
            Assert.Empty(source.Reads);
        } else {
            TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(source, "", destination, "", options);
            Assert.True(result.IsSuccess);
            Assert.Equal("payload", File.ReadAllText(Path.Combine(destinationRoot, @"literal\name.txt")));
            Assert.Contains(@"literal\name.txt", source.Reads);
            Assert.DoesNotContain("literal/name.txt", source.Reads);
        }
        Assert.Equal("garbage", File.ReadAllText(Path.Combine(destinationRoot, "literal", "name.txt")));
    }

    [Fact]
    public async Task MirrorNeverDeletesTheCaseAliasOfTheFileItJustCopied() {
        bool insensitive = IsCaseInsensitive();
        string destinationRoot = Path.Combine(_root, "mirror");
        Directory.CreateDirectory(destinationRoot);
        File.WriteAllText(Path.Combine(destinationRoot, "report.txt"), "old");
        ExactEndpoint source = new(("Report.txt", "replacement"));

        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(source, "",
            new FileSystemTransferEndpoint(destinationRoot), "", new TransferettoSyncOptions {
                Direction = TransferettoSyncDirection.Download, Mode = TransferettoSyncMode.Mirror,
                Comparison = TransferettoSyncComparison.Always
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("replacement", File.ReadAllText(Path.Combine(destinationRoot, "Report.txt")));
        Assert.Single(Directory.GetFiles(destinationRoot));
        if (insensitive) { Assert.DoesNotContain(result.Items, item => item.WasDeleted); }
        else { Assert.False(File.Exists(Path.Combine(destinationRoot, "report.txt"))); }
    }

    [Fact]
    public async Task ContradictoryOrdinalMirrorFailsBeforeChangingAnAliasedDestination() {
        if (!IsCaseInsensitive()) { return; }
        string destinationRoot = Path.Combine(_root, "mirror");
        Directory.CreateDirectory(destinationRoot);
        File.WriteAllText(Path.Combine(destinationRoot, "report.txt"), "old");
        await Assert.ThrowsAsync<InvalidDataException>(() => TransferettoEndpointSync.SyncAsync(
            new ExactEndpoint(("Report.txt", "replacement")), "", new FileSystemTransferEndpoint(destinationRoot), "",
            new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror,
                PathComparison = TransferettoSyncPathComparison.Ordinal }));
        Assert.Equal("old", File.ReadAllText(Path.Combine(destinationRoot, "report.txt")));
    }

    [Fact]
    public async Task OverlappingEndpointRootsCannotScheduleTwoWritesToOneFile() {
        string destinationRoot = Path.Combine(_root, "batch");
        Directory.CreateDirectory(Path.Combine(destinationRoot, "sub"));
        string target = Path.Combine(destinationRoot, "sub", "item.txt");
        File.WriteAllText(target, "original");
        ExactEndpoint source = new(("one", "first"), ("two", "second"));
        FileSystemTransferEndpoint outer = new(destinationRoot);
        FileSystemTransferEndpoint inner = new(Path.Combine(destinationRoot, "sub"));
        TransferCopyOptions overwrite = new() { WriteOptions = new() { Mode = TransferWriteMode.Overwrite } };
        TransferBatchItem[] items = {
            new(source, "one", outer, "sub/item.txt") { Options = overwrite },
            new(source, "two", inner, "item.txt") { Options = overwrite }
        };
        await Assert.ThrowsAsync<ArgumentException>(() => TransferEngine.CopyBatchAsync(items));
        Assert.Empty(source.Reads);
        Assert.Equal("original", File.ReadAllText(target));
    }

    [Theory]
    [InlineData("*")]
    [InlineData("?")]
    public void FtpManifestTraversesWildcardDirectoryNamesExactlyOnce(string name) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }
        List<string> calls = new();
        IReadOnlyList<TransferettoRemoteItem> ListExact(string path) {
            calls.Add(path);
            Assert.True(calls.Count <= 2, "A literal directory must not relist its parent.");
            return path == "/root" ? new[] { new TransferettoRemoteItem {
                Name = name, FullName = "/root/" + name, Type = FtpObjectType.Directory
            } } : new[] { new TransferettoRemoteItem {
                Name = "file", FullName = path + "/file", Type = FtpObjectType.File, Size = 3
            } };
        }
        IReadOnlyList<TransferettoSyncEntry> manifest = TransferettoClient.BuildFtpRemoteSyncManifest(ListExact, "/root", _root);
        Assert.Equal(new[] { "/root", "/root/" + name }, calls);
        Assert.Equal(new[] { name, name + "/file" }, manifest.Select(item => item.RelativePath));
    }

    [Fact]
    public void FtpManifestCancellationStopsBeforeListingAnotherDirectory() {
        using CancellationTokenSource cancellation = new();
        int calls = 0;
        IReadOnlyList<TransferettoRemoteItem> ListExact(string path) {
            calls++;
            cancellation.Cancel();
            return new[] { new TransferettoRemoteItem { Name = "child", FullName = "/root/child", Type = FtpObjectType.Directory } };
        }
        Assert.Throws<OperationCanceledException>(() => TransferettoClient.BuildFtpRemoteSyncManifest(ListExact, "/root", _root, cancellation.Token));
        Assert.Equal(1, calls);
    }

    private bool IsCaseInsensitive() {
        string probe = Path.Combine(_root, "case-probe");
        File.WriteAllText(probe, "probe");
        bool result = File.Exists(Path.Combine(_root, "CASE-PROBE"));
        Assert.Equal(result ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal,
            TransferFileSystem.GetPathComparison(_root));
        File.Delete(probe);
        return result;
    }

    public void Dispose() { Directory.Delete(_root, recursive: true); }

    private sealed class ExactEndpoint : ITransferEndpoint {
        private readonly Dictionary<string, byte[]> _files;
        internal ExactEndpoint(params (string Path, string Content)[] files) {
            _files = files.ToDictionary(file => file.Path, file => Encoding.UTF8.GetBytes(file.Content), StringComparer.Ordinal);
        }
        internal List<string> Reads { get; } = new();
        public string Scheme => "sftp";
        public string DisplayName => "synthetic://exact";
        public TransferEndpointCapabilities Capabilities => TransferEndpointCapabilities.List | TransferEndpointCapabilities.Read | TransferEndpointCapabilities.Inspect;
        private TransferItem Item(string path) => new() { Path = path, Length = _files[path].Length, ETag = path };
        public Task<IReadOnlyList<TransferItem>> ListAsync(string prefix, bool recursive = true, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TransferItem>>(_files.Keys.Select(Item).ToArray());
        public Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) =>
            Task.FromResult<TransferItem?>(_files.ContainsKey(path) ? Item(path) : null);
        public Task<TransferReadHandle> OpenReadAsync(string path, CancellationToken cancellationToken = default) {
            Reads.Add(path);
            return Task.FromResult(new TransferReadHandle(Item(path), new MemoryStream(_files[path], writable: false)));
        }
        public Task<TransferWriteResult> WriteAsync(string path, Stream content, long? length, TransferWriteOptions? options = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
