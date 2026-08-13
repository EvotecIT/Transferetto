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

        Assert.Throws<IOException>(() => Commit(
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

        IOException exception = Assert.Throws<IOException>(() => Commit(
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
    public void Overwrite_RestoresMostRecentConcurrentDestinationWhenCommitFails() {
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
        Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        Assert.Equal("racer", operations.Files["target"]);
        Assert.Equal("new", operations.Files["temp"]);
        Assert.DoesNotContain(operations.Files.Keys, path => path.EndsWith(".previous", StringComparison.Ordinal));
    }

    [Fact]
    public void Overwrite_ReportsEveryBackupWhenConcurrentDestinationBlocksRecovery() {
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
                operations.Files["target"] = commitAttempts == 1 ? "first-racer" : "second-racer";
                throw new IOException("Injected commit failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        Assert.Equal("second-racer", operations.Files["target"]);
        KeyValuePair<string, string>[] retained = operations.Files
            .Where(pair => pair.Key.EndsWith(".previous", StringComparison.Ordinal))
            .OrderBy(pair => pair.Value, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(2, retained.Length);
        Assert.Equal(new[] { "first-racer", "old" }, retained.Select(pair => pair.Value));
        Assert.All(retained, pair => Assert.Contains(pair.Key, exception.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void Overwrite_TracksMoveAsideWhenResponseIsLostAndDestinationReappears() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        bool responseLost = false;
        operations.OnRename = (source, destination, posix) => {
            if (posix) {
                throw new NotSupportedException("POSIX rename is unavailable.");
            }
            if (!responseLost && source == "target" && destination.EndsWith(".previous", StringComparison.Ordinal)) {
                responseLost = true;
                operations.Files.Remove(source);
                operations.Files[destination] = "old";
                operations.Files[source] = "racer";
                throw new IOException("Rename response was lost.");
            }
        };

        bool committed = Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite);

        Assert.True(committed);
        Assert.Equal("new", operations.Files["target"]);
        Assert.DoesNotContain(operations.Files.Keys, path => path.EndsWith(".previous", StringComparison.Ordinal));
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

        bool committed = Commit(
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
    public void SkipIfExists_ReportsRetainedTemporaryItemWhenCleanupFails() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "existing";
        operations.OnDelete = path => {
            if (path == "temp") {
                throw new IOException("Injected cleanup failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.SkipIfExists));

        Assert.Contains("temp", exception.Message, StringComparison.Ordinal);
        Assert.Equal("new", operations.Files["temp"]);
        Assert.Equal("existing", operations.Files["target"]);
    }

    [Fact]
    public void SkipIfExists_ReportsRetainedTemporaryItemAfterRenameRace() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.OnRename = (source, destination, _) => {
            if (source == "temp" && destination == "target") {
                operations.Files["target"] = "racer";
                throw new IOException("Destination exists.");
            }
        };
        operations.OnDelete = path => {
            if (path == "temp") {
                throw new IOException("Injected cleanup failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.SkipIfExists));

        Assert.Contains("temp", exception.Message, StringComparison.Ordinal);
        Assert.Equal("new", operations.Files["temp"]);
        Assert.Equal("racer", operations.Files["target"]);
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

        IOException exception = Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.FailIfExists));

        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("racer", operations.Files["target"]);
        Assert.Equal("new", operations.Files["temp"]);
    }

    [Theory]
    [InlineData(TransferWriteMode.SkipIfExists)]
    [InlineData(TransferWriteMode.FailIfExists)]
    public void NonOverwrite_FailsClosedWithoutNoClobberRename(TransferWriteMode mode) {
        FakeSftpCommitOperations operations = new() {
            SupportsNoClobberRename = false
        };
        operations.Files["temp"] = "new";

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            mode));

        Assert.Contains("atomic create-if-absent", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("new", operations.Files["temp"]);
        Assert.False(operations.Files.ContainsKey("target"));
    }

    [Fact]
    public void Overwrite_RetainsBackupWhenNoClobberRecoveryIsUnavailable() {
        FakeSftpCommitOperations operations = new() {
            SupportsNoClobberRename = false
        };
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        operations.OnRename = (source, destination, posix) => {
            if (posix) {
                throw new NotSupportedException("Atomic rename is unavailable.");
            }
            if (source == "temp" && destination == "target") {
                throw new IOException("Injected commit failure.");
            }
        };

        IOException exception = Assert.Throws<IOException>(() => Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite));

        KeyValuePair<string, string> retained = Assert.Single(
            operations.Files,
            pair => pair.Key.EndsWith(".previous", StringComparison.Ordinal));
        Assert.Equal("old", retained.Value);
        Assert.Contains(retained.Key, exception.Message, StringComparison.Ordinal);
        Assert.False(operations.Files.ContainsKey("target"));
        Assert.Equal("new", operations.Files["temp"]);
    }

    [Theory]
    [InlineData(TransferWriteMode.SkipIfExists)]
    [InlineData(TransferWriteMode.FailIfExists)]
    public void NonOverwrite_RecognizesSuccessfulRenameAfterResponseLoss(TransferWriteMode mode) {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.OnRename = (source, destination, _) => {
            if (source == "temp" && destination == "target") {
                operations.Files.Remove(source);
                operations.Files[destination] = "new";
                throw new IOException("Rename response was lost.");
            }
        };

        bool committed = Commit(operations, "temp", "target", "target", mode);

        Assert.True(committed);
        Assert.Equal("new", operations.Files["target"]);
        Assert.False(operations.Files.ContainsKey("temp"));
    }

    [Fact]
    public void Overwrite_RecognizesSuccessfulAtomicRenameAfterResponseLoss() {
        FakeSftpCommitOperations operations = new();
        operations.Files["temp"] = "new";
        operations.Files["target"] = "old";
        operations.OnRename = (source, destination, posix) => {
            if (posix) {
                operations.Files.Remove(source);
                operations.Files[destination] = "new";
                throw new IOException("Rename response was lost.");
            }
        };

        bool committed = Commit(
            operations,
            "temp",
            "target",
            "target",
            TransferWriteMode.Overwrite);

        Assert.True(committed);
        Assert.Equal("new", operations.Files["target"]);
        Assert.False(operations.Files.ContainsKey("temp"));
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

    [Fact]
    public void DirectoryCreation_ToleratesAnotherWriterWinningTheRace() {
        FakeSftpDirectoryOperations operations = new();
        operations.OnCreate = path => {
            operations.Directories.Add(path);
            throw new IOException("Directory already exists.");
        };

        SftpTransferDirectory.Ensure(operations, "/home/user/incoming");

        Assert.Contains("/home", operations.Directories);
        Assert.Contains("/home/user", operations.Directories);
        Assert.Contains("/home/user/incoming", operations.Directories);
    }

    [Fact]
    public void DirectoryCreation_PreservesFailureWhenPathIsStillMissing() {
        FakeSftpDirectoryOperations operations = new() {
            OnCreate = _ => throw new IOException("Permission denied.")
        };

        Assert.Throws<IOException>(() =>
            SftpTransferDirectory.Ensure(operations, "/restricted/incoming"));
    }

    private sealed class TrackingStream : MemoryStream {
        internal bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing) {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private static bool Commit(
        FakeSftpCommitOperations operations,
        string temporaryPath,
        string destinationPath,
        string relativePath,
        TransferWriteMode mode) => ProtocolTransferCommit.Commit(
            operations,
            temporaryPath,
            destinationPath,
            relativePath,
            mode,
            "SFTP");

    private sealed class FakeSftpCommitOperations : IProtocolTransferCommitOperations {
        internal Dictionary<string, string> Files { get; } = new(StringComparer.Ordinal);

        internal Action<string, string, bool>? OnRename { get; set; }

        internal Action<string>? OnDelete { get; set; }

        public bool SupportsNoClobberRename { get; set; } = true;

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

    private sealed class FakeSftpDirectoryOperations : ISftpTransferDirectoryOperations {
        internal HashSet<string> Directories { get; } = new(StringComparer.Ordinal);

        internal Action<string>? OnCreate { get; set; }

        public bool IsDirectory(string path) => Directories.Contains(path);

        public void CreateDirectory(string path) {
            OnCreate?.Invoke(path);
            Directories.Add(path);
        }
    }
}
