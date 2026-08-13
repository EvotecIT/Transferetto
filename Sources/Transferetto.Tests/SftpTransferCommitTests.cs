using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class SftpTransferCommitTests {
    [Fact]
    public void Overwrite_RestoresOriginalWhenFallbackCommitFails() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        operations.OnRename = (source, destination, posix) => {
            if (posix) {
                throw new NotSupportedException("POSIX rename is unavailable.");
            }
            if (source == "temp" && destination == "target") {
                throw new IOException("Injected commit failure.");
            }
        };

        Assert.Throws<IOException>(() => SftpTransferCommit.Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        Assert.Equal("old", operations.Files["target"]);
        Assert.Equal("new", operations.Files["temp"]);
        Assert.DoesNotContain(operations.Files.Keys, path => path.EndsWith(".previous", StringComparison.Ordinal));
    }

    [Fact]
    public void Overwrite_ReportsBackupWhenCleanupFailsAfterSuccessfulCommit() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        operations.OnRename = (_, _, posix) => {
            if (posix) {
                throw new NotSupportedException("POSIX rename is unavailable.");
            }
        };
        operations.OnDelete = path => {
            if (path.EndsWith(".previous", StringComparison.Ordinal)) {
                throw new IOException("Injected cleanup failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => SftpTransferCommit.Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        string retainedPath = Assert.Single(
            operations.Files.Keys,
            path => path.EndsWith(".previous", StringComparison.Ordinal));
        Assert.Equal("new", operations.Files["target"]);
        Assert.Equal("old", operations.Files[retainedPath]);
        Assert.Contains(retainedPath, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Overwrite_ReportsEveryRetainedBackupAfterRaceAndRollback() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        int commitAttempts = 0;
        operations.OnRename = (source, destination, posix) => {
            if (posix) {
                throw new NotSupportedException("POSIX rename is unavailable.");
            }
            if (source == "temp" && destination == "target") {
                commitAttempts++;
                if (commitAttempts == 1) {
                    operations.Files["target"] = "racer";
                }
                throw new IOException("Injected commit failure.");
            }
        };
        operations.OnDelete = path => {
            if (operations.Files.TryGetValue(path, out string? content) && content == "racer") {
                throw new IOException("Injected cleanup failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => SftpTransferCommit.Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        Assert.Equal("old", operations.Files["target"]);
        KeyValuePair<string, string> retained = Assert.Single(
            operations.Files,
            pair => pair.Value == "racer");
        string retainedPath = retained.Key;
        Assert.Contains(retainedPath, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SkipIfExists_ReturnsSkippedWhenDestinationWinsRenameRace() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.OnRename = (source, destination, _) => {
            if (source == "temp" && destination == "target") {
                operations.Files["target"] = "racer";
                throw new IOException("Destination exists.");
            }
        };

        bool committed = SftpTransferCommit.Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.SkipIfExists);

        Assert.False(committed);
        Assert.Equal("racer", operations.Files["target"]);
        Assert.False(operations.Files.ContainsKey("temp"));
    }

    [Fact]
    public void FailIfExists_ReportsCollisionWhenDestinationWinsRenameRace() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.OnRename = (source, destination, _) => {
            if (source == "temp" && destination == "target") {
                operations.Files["target"] = "racer";
                throw new IOException("Destination exists.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => SftpTransferCommit.Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.FailIfExists));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("racer", operations.Files["target"]);
        Assert.Equal("new", operations.Files["temp"]);
    }

    [Fact]
    public void OpenRead_DisposesAcquiredStreamWhenCancellationWinsOwnershipTransfer() {
        TrackingStream stream = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() => ProtocolTransferEndpointResource.OpenRead(
            () => stream,
            new TransferItem { Path = "source.bin" },
            cancellation.Token));

        Assert.True(stream.WasDisposed);
    }

    [Fact]
    public void ConnectingConstructors_ValidatePrefixBeforeAttemptingConnection() {
        TransferettoFtpConnectionOptions ftpOptions = new() {
            Server = "127.0.0.1",
            Port = 1
        };
        TransferettoSftpConnectionOptions sftpOptions = new() {
            Server = "127.0.0.1",
            Port = 1,
            UserName = "unused",
            Password = "unused"
        };

        Assert.Throws<ArgumentException>(() => new FtpTransferEndpoint(ftpOptions, "../outside"));
        Assert.Throws<ArgumentException>(() => new SftpTransferEndpoint(sftpOptions, "../outside"));
    }

    private sealed class TrackingStream : MemoryStream {
        internal bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing) {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class FakeSftpCommitOperations : ISftpTransferCommitOperations {
        internal Dictionary<string, string> Files { get; } = new(StringComparer.Ordinal);

        internal Action<string, string, bool>? OnRename { get; set; }

        internal Action<string>? OnDelete { get; set; }

        public bool Exists(string path) => Files.ContainsKey(path);

        public void Delete(string path) {
            OnDelete?.Invoke(path);
            Files.Remove(path);
        }

        public void Rename(string sourcePath, string destinationPath, bool posix) {
            OnRename?.Invoke(sourcePath, destinationPath, posix);
            if (!Files.TryGetValue(sourcePath, out string? content)) {
                throw new FileNotFoundException("Source does not exist.", sourcePath);
            }
            if (!posix && Files.ContainsKey(destinationPath)) {
                throw new IOException("Destination exists.");
            }
            Files.Remove(sourcePath);
            Files[destinationPath] = content;
        }
    }
}
