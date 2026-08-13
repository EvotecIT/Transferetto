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
    public async Task ReadTrackingStream_ReportsConsumedBytesAndLeavesSourceOpen() {
        byte[] content = Encoding.UTF8.GetBytes("actual-upload-content");
        using MemoryStream source = new(content);
        using TransferReadTrackingStream tracked = new(source, leaveOpen: true);
        using MemoryStream destination = new();

        await tracked.CopyToAsync(destination);

        Assert.Equal(content.LongLength, tracked.BytesRead);
        Assert.Equal(content, destination.ToArray());
        tracked.Dispose();
        Assert.True(source.CanRead);
    }

    [Fact]
    public void ReadTrackingStream_PreservesSeekabilityWithoutDoubleCountingRetries() {
        using MemoryStream source = new(Encoding.UTF8.GetBytes("prefix-payload"));
        source.Position = "prefix-".Length;
        using TransferReadTrackingStream tracked = new(source, leaveOpen: true);
        byte[] buffer = new byte["payload".Length];

        Assert.True(tracked.CanSeek);
        Assert.Equal(source.Length, tracked.Length);
        Assert.Equal("prefix-".Length, tracked.Position);
        Assert.Equal(buffer.Length, tracked.Read(buffer, 0, buffer.Length));
        Assert.Equal(buffer.LongLength, tracked.BytesRead);

        Assert.Equal("prefix-".Length, tracked.Seek("prefix-".Length, SeekOrigin.Begin));
        Assert.Equal(buffer.Length, tracked.Read(buffer, 0, buffer.Length));
        Assert.Equal(buffer.LongLength, tracked.BytesRead);
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
    public async Task CopyAsync_FiltersProviderSpecificSourceMetadata() {
        byte[] content = Encoding.UTF8.GetBytes("metadata");
        RecordingEndpoint destination = new();
        TransferReceipt receipt = await TransferEngine.CopyAsync(
            new MetadataSourceEndpoint(content),
            "source.bin",
            destination,
            "destination.bin");

        Assert.Equal(TransferReceiptOutcome.Copied, receipt.Outcome);
        Assert.NotNull(destination.Options);
        Assert.Equal("application/octet-stream", destination.Options!.ContentType);
        Assert.Equal("portable", destination.Options.Metadata["evidence_id"]);
        Assert.DoesNotContain("build-id", destination.Options.Metadata.Keys);
    }

    [Fact]
    public async Task CopyAsync_DropsAutomaticMetadataForDestinationWithoutMetadataCapability() {
        RecordingEndpoint destination = new(supportsMetadata: false);

        TransferReceipt receipt = await TransferEngine.CopyAsync(
            new MetadataSourceEndpoint(Encoding.UTF8.GetBytes("metadata")),
            "source.bin",
            destination,
            "destination.bin");

        Assert.Equal(TransferReceiptOutcome.Copied, receipt.Outcome);
        Assert.NotNull(destination.Options);
        Assert.Null(destination.Options!.ContentType);
        Assert.Empty(destination.Options.Metadata);
    }

    [Fact]
    public async Task CopyAsync_TreatsNegativeProviderLengthAsUnknown() {
        byte[] content = Encoding.UTF8.GetBytes("unknown-length");
        RecordingEndpoint destination = new();

        TransferReceipt receipt = await TransferEngine.CopyAsync(
            new MetadataSourceEndpoint(content, reportedLength: -1),
            "source.bin",
            destination,
            "destination.bin");

        Assert.Equal(content.LongLength, receipt.BytesTransferred);
        Assert.Null(destination.Length);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(8)]
    public async Task CopyAsync_RejectsChangedSourceLengthBeforeDestinationCommits(long reportedLength) {
        byte[] content = Encoding.UTF8.GetBytes("length");
        RecordingEndpoint destination = new();

        await Assert.ThrowsAsync<EndOfStreamException>(() => TransferEngine.CopyAsync(
            new MetadataSourceEndpoint(content, reportedLength),
            "source.bin",
            destination,
            "destination.bin"));

        Assert.False(destination.Committed);
    }

    [Fact]
    public async Task CopyAsync_RejectsExplicitMetadataForDestinationWithoutMetadataCapability() {
        RecordingEndpoint destination = new(supportsMetadata: false);
        TransferCopyOptions options = new();
        options.WriteOptions.Metadata["evidence_id"] = "explicit";

        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            TransferEngine.CopyAsync(
                new MetadataSourceEndpoint(Encoding.UTF8.GetBytes("metadata")),
                "source.bin",
                destination,
                "destination.bin",
                options));

        Assert.Contains("explicitly requested", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(destination.Options);
    }

    [Fact]
    public async Task CopyAsync_RejectsExplicitContentTypeForDestinationWithoutMetadataCapability() {
        RecordingEndpoint destination = new(supportsMetadata: false);
        TransferCopyOptions options = new() {
            WriteOptions = new TransferWriteOptions { ContentType = "application/json" }
        };

        await Assert.ThrowsAsync<NotSupportedException>(() => TransferEngine.CopyAsync(
            new MetadataSourceEndpoint(Encoding.UTF8.GetBytes("metadata")),
            "source.bin",
            destination,
            "destination.bin",
            options));
        Assert.Null(destination.Options);
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

    [Theory]
    [InlineData(3)]
    [InlineData(8)]
    public async Task FileSystemEndpoint_PreservesExistingItemWhenContentLengthChanges(long expectedLength) {
        FileSystemTransferEndpoint endpoint = new(_root);
        string targetPath = Path.Combine(_root, "item.txt");
        File.WriteAllText(targetPath, "original");

        await Assert.ThrowsAsync<EndOfStreamException>(() => endpoint.WriteAsync(
            "item.txt",
            new MemoryStream(Encoding.UTF8.GetBytes("length")),
            expectedLength,
            new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }));

        Assert.Equal("original", File.ReadAllText(targetPath));
    }

    [Theory]
    [InlineData(TransferWriteMode.SkipIfExists)]
    [InlineData(TransferWriteMode.FailIfExists)]
    public async Task FileSystemEndpoint_DoesNotOverwriteAConcurrentDestination(
        TransferWriteMode writeMode) {
        FileSystemTransferEndpoint endpoint = new(_root);
        using CoordinatedReadStream content = new(Encoding.UTF8.GetBytes("incoming"));
        Task<TransferWriteResult> write = endpoint.WriteAsync(
            "race.txt",
            content,
            8,
            new TransferWriteOptions { Mode = writeMode });

        Task readStarted = await Task.WhenAny(content.ReadStarted, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(content.ReadStarted, readStarted);
        File.WriteAllText(Path.Combine(_root, "race.txt"), "concurrent");
        content.Release();

        if (writeMode == TransferWriteMode.SkipIfExists) {
            TransferWriteResult result = await write;
            Assert.False(result.WasWritten);
        } else {
            await Assert.ThrowsAsync<IOException>(async () => await write);
        }
        Assert.Equal("concurrent", File.ReadAllText(Path.Combine(_root, "race.txt")));
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task FileSystemEndpoint_RejectsDirectorySymlinkTraversal() {
        string outsideRoot = Path.Combine(
            Path.GetTempPath(),
            "Transferetto.Tests.Outside",
            Guid.NewGuid().ToString("N"));
        string linkPath = Path.Combine(_root, "linked");
        Directory.CreateDirectory(outsideRoot);
        File.WriteAllText(Path.Combine(outsideRoot, "secret.txt"), "outside");
        try {
            try {
                Directory.CreateSymbolicLink(linkPath, outsideRoot);
            } catch (UnauthorizedAccessException) {
                return;
            } catch (PlatformNotSupportedException) {
                return;
            }

            FileSystemTransferEndpoint endpoint = new(_root);
            await Assert.ThrowsAsync<IOException>(() => endpoint.GetItemAsync("linked/secret.txt"));
            await Assert.ThrowsAsync<IOException>(() => endpoint.OpenReadAsync("linked/secret.txt"));
            await Assert.ThrowsAsync<IOException>(() => endpoint.WriteAsync(
                "linked/new.txt",
                new MemoryStream(new byte[] { 1 }),
                1));
            await Assert.ThrowsAsync<IOException>(() => endpoint.DeleteAsync("linked/secret.txt"));
            await Assert.ThrowsAsync<IOException>(() => endpoint.ListAsync(string.Empty));
            Assert.False(File.Exists(Path.Combine(outsideRoot, "new.txt")));
        } finally {
            if (Directory.Exists(linkPath)) {
                Directory.Delete(linkPath);
            }
            if (Directory.Exists(outsideRoot)) {
                Directory.Delete(outsideRoot, recursive: true);
            }
        }
    }
#endif

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

    private sealed class CoordinatedReadStream : Stream {
        private readonly MemoryStream _inner;
        private readonly TaskCompletionSource<bool> _readStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal CoordinatedReadStream(byte[] content) {
            _inner = new MemoryStream(content);
        }

        internal Task ReadStarted => _readStarted.Task;

        internal void Release() => _release.TrySetResult(true);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            _readStarted.TrySetResult(true);
            _release.Task.GetAwaiter().GetResult();
            return _inner.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            _readStarted.TrySetResult(true);
            await _release.Task.ConfigureAwait(false);
            return await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _release.TrySetResult(true);
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class MetadataSourceEndpoint : ITransferEndpoint {
        private readonly byte[] _content;
        private readonly long? _reportedLength;

        internal MetadataSourceEndpoint(byte[] content, long? reportedLength = null) {
            _content = content;
            _reportedLength = reportedLength;
        }

        public string Scheme => "source";
        public string DisplayName => "source://test/";
        public TransferEndpointCapabilities Capabilities => TransferEndpointCapabilities.Read;

        public Task<TransferReadHandle> OpenReadAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new TransferReadHandle(
                new TransferItem {
                    Path = path,
                    Length = _reportedLength ?? _content.LongLength,
                    ContentType = "application/octet-stream",
                    Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                        ["build-id"] = "provider-specific",
                        ["evidence_id"] = "portable"
                    }
                },
                new MemoryStream(_content, writable: false)));

        public Task<TransferItem?> GetItemAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TransferItem>> ListAsync(
            string prefix,
            bool recursive = true,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TransferWriteResult> WriteAsync(
            string path,
            Stream content,
            long? length,
            TransferWriteOptions? options = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingEndpoint : ITransferEndpoint {
        private readonly bool _supportsMetadata;

        internal RecordingEndpoint(bool supportsMetadata = true) {
            _supportsMetadata = supportsMetadata;
        }

        internal TransferWriteOptions? Options { get; private set; }

        internal long? Length { get; private set; }

        internal bool Committed { get; private set; }

        public string Scheme => "destination";
        public string DisplayName => "destination://test/";
        public TransferEndpointCapabilities Capabilities => _supportsMetadata
            ? TransferEndpointCapabilities.Write | TransferEndpointCapabilities.Metadata
            : TransferEndpointCapabilities.Write;

        public async Task<TransferWriteResult> WriteAsync(
            string path,
            Stream content,
            long? length,
            TransferWriteOptions? options = null,
            CancellationToken cancellationToken = default) {
            Options = options;
            Length = length;
            using MemoryStream sink = new();
            await content.CopyToAsync(sink, 81920, cancellationToken);
            Committed = true;
            return new TransferWriteResult(new TransferItem {
                Path = path,
                Length = sink.Length
            }, wasWritten: true);
        }

        public Task<TransferItem?> GetItemAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TransferItem>> ListAsync(
            string prefix,
            bool recursive = true,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TransferReadHandle> OpenReadAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
