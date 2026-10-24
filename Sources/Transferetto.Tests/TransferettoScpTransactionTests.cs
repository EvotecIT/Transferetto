using System.Text;
using System.Runtime.InteropServices;
using Renci.SshNet;

namespace Transferetto.Tests;

public sealed class TransferettoScpTransactionTests : IDisposable {
    private readonly string _parent = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
    public TransferettoScpTransactionTests() { Directory.CreateDirectory(_parent); }

    [Theory]
    [InlineData(false, "C0644 1 ../escape\nx\0")]
    [InlineData(true, "C0644 1 ../escape\nx\0")]
    [InlineData(false, "C0644 1 target\nx\0")]
    [InlineData(true, "C0644 1 target\nx\0")]
    [InlineData(false, "C0644 3 broken\nxy")]
    [InlineData(true, "C0644 3 broken\nxy")]
    public async Task RejectedDirectoryLeavesNoNewFilesAndPreservesExistingContents(bool existing, string failure) {
        string selected = Path.Combine(_parent, "download");
        DateTime modified = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        if (existing) {
            Directory.CreateDirectory(selected);
            File.WriteAllText(Path.Combine(selected, "target"), "original");
            File.WriteAllText(Path.Combine(selected, "unrelated"), "keep");
            Directory.SetLastWriteTimeUtc(selected, modified);
        }
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0D0755 0 child\nE\n" + failure));
        using MemoryStream acks = new();
        Task Receive() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None);
        if (failure.Contains("broken")) { await Assert.ThrowsAsync<EndOfStreamException>(Receive); }
        else { await Assert.ThrowsAsync<InvalidDataException>(Receive); }
        if (existing) {
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "target")));
            Assert.Equal("keep", File.ReadAllText(Path.Combine(selected, "unrelated")));
            Assert.Equal(2, Directory.GetFileSystemEntries(selected).Length);
            Assert.Equal(modified, Directory.GetLastWriteTimeUtc(selected));
            Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
        } else {
            Assert.False(Directory.Exists(selected));
            Assert.Empty(Directory.GetFileSystemEntries(_parent));
        }
    }

    [Fact]
    public async Task CancellationAfterReceivingContentLeavesNoDirectoryOrStaging() {
        string selected = Path.Combine(_parent, "download");
        using CancellationTokenSource cancellation = new();
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 file\nnew\0E\n"));
        using MemoryStream acks = new();
        ScpDirectoryReceiver receiver = new(records, acks, selected, (_, _, _) => cancellation.Cancel());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => receiver.ReceiveAsync(cancellation.Token));
        Assert.False(Directory.Exists(selected));
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task FinalAcknowledgementFailureDiscardsTheWholeDirectory() {
        string selected = Path.Combine(_parent, "download");
        using MemoryStream records = Wire();
        using FailingAcknowledgements acks = new();
        await Assert.ThrowsAsync<IOException>(() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None));
        Assert.Equal(4, acks.Writes);
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task RemoteCompletionFailureOccursBeforeAnyReplacementIsPublished() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "target"), "original");
        using MemoryStream records = Wire();
        using MemoryStream acks = new();
        bool checkedCompletion = false;
        await Assert.ThrowsAsync<IOException>(() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None, () => {
            checkedCompletion = true;
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "target")));
            throw new IOException("Server exit status is unsuccessful");
        }));
        Assert.True(checkedCompletion);
        Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "target")));
        Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task SuccessfulDirectoryMergesFilesAndEmptyDirectoriesOnlyAfterCompletion() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "target"), "original");
        File.WriteAllText(Path.Combine(selected, "unrelated"), "keep");
        using MemoryStream records = new(Encoding.UTF8.GetBytes("T1700000000 0 1700000000 0\nD0755 0 root\nT1600000000 0 1600000000 0\nC0644 3 target\nnew\0D0755 0 empty\nE\nE\n"));
        using MemoryStream acks = new();
        await new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None, () => {
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "target")));
            Assert.False(Directory.Exists(Path.Combine(selected, "empty")));
            return Task.CompletedTask;
        });
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "target")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(selected, "unrelated")));
        Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(selected, "empty")));
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1600000000).UtcDateTime, File.GetLastWriteTimeUtc(Path.Combine(selected, "target")));
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000).UtcDateTime, Directory.GetLastWriteTimeUtc(selected));
        Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task FailedCommitRestoresEarlierReplacementsAndRemovesNewDirectories() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "target"), "original");
        File.WriteAllText(Path.Combine(selected, "locked"), "keep");
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0D0755 0 child\nE\nC0644 3 locked\ntwo\0E\n"));
        using MemoryStream acks = new();
        // Windows denies renaming an open, non-shared destination. On Unix, remove
        // the second owned staged file to exercise the same post-first-write I/O failure.
        FileStream? locked = null;
        try {
            await Assert.ThrowsAnyAsync<IOException>(() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None, () => {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    locked = new FileStream(Path.Combine(selected, "locked"), FileMode.Open, FileAccess.Read, FileShare.None);
                } else {
                    string stage = Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*"));
                    string[] files = Directory.GetFiles(stage);
                    Assert.Equal(2, files.Length);
                    File.Delete(Assert.Single(files, file => File.ReadAllText(file) == "two"));
                }
                return Task.CompletedTask;
            }));
        } finally { locked?.Dispose(); }
        Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "target")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(selected, "locked")));
        Assert.Equal(2, Directory.GetFileSystemEntries(selected).Length);
        Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public void DisconnectedSessionDoesNotCreateTheSelectedDirectory() {
        string selected = Path.Combine(_parent, "download");
        using TransferettoScpSession session = new(new ScpClient(new PasswordConnectionInfo("example.invalid", "synthetic", "synthetic"), RemotePathTransformation.ShellQuote));
        Assert.Throws<InvalidOperationException>(() => TransferettoClient.DownloadScpDirectory(session, "source", selected));
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CommitFailureRemovesNewRootAndItsNewParents(bool trailingSeparator) {
        string selected = Path.Combine(_parent, "missing", "download");
        if (trailingSeparator) { selected += Path.DirectorySeparatorChar; }
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 first\none\0C0644 3 second\ntwo\0E\n"));
        using MemoryStream acks = new();
        await Assert.ThrowsAsync<FileNotFoundException>(() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None, () => {
            string stage = Assert.Single(Directory.GetDirectories(_parent, ".transferetto-scp-*"));
            File.Delete(Assert.Single(Directory.GetFiles(stage), file => File.ReadAllText(file) == "two"));
            return Task.CompletedTask;
        }));
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task StagingDirectoryCannotBeSelectedAsAnIncomingMember() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        using (LocalDirectoryTransaction transaction = new(selected)) {
            using MemoryStream content = new(new byte[] { 1 });
            await transaction.AddFileAsync("other", content, 1, null, null, CancellationToken.None);
            string ownStage = Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*"));
            transaction.AddDirectory(Path.GetFileName(ownStage), null, null);
            await Assert.ThrowsAsync<InvalidDataException>(() => transaction.CommitAsync(CancellationToken.None));
        }
        Assert.Empty(Directory.GetFileSystemEntries(selected));
        Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
    }

    [Fact]
    public async Task RecordsAfterTheSelectedRootAreRejectedBeforePublication() {
        string selected = Path.Combine(_parent, "download");
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0E\nC0644 3 extra\nbad\0"));
        using MemoryStream acks = new();
        await Assert.ThrowsAsync<InvalidDataException>(() => new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None));
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    private static MemoryStream Wire() => new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0E\n"));

    [Fact]
    public async Task MissingCleanupStagePreservesTheReceiveErrorAndRestoresParentTime() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        DateTime modified = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        Directory.SetLastWriteTimeUtc(selected, modified);
        IOException expected = new("Remote command failed");
        using MemoryStream records = Wire();
        using MemoryStream acks = new();
        IOException actual = await Assert.ThrowsAsync<IOException>(() =>
            new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None, () => {
                // An external cleanup can remove our stage before the receiver disposes it.
                Directory.Delete(Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*")), recursive: true);
                Directory.SetLastWriteTimeUtc(selected, modified.AddDays(1));
                throw expected;
            }));
        Assert.Same(expected, actual);
        Assert.Empty(Directory.GetFileSystemEntries(selected));
        Assert.Equal(modified, Directory.GetLastWriteTimeUtc(selected));
    }

    [Fact]
    public async Task MissingCleanupStageDoesNotFailPublicationOrSkipRemoteDirectoryTime() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        DateTime modified = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        using LocalDirectoryTransaction transaction = new(selected);
        transaction.AddDirectory("", modified, null);
        using MemoryStream content = new(Encoding.UTF8.GetBytes("new"));
        await transaction.AddFileAsync("target", content, content.Length, null, null, CancellationToken.None);
        string stage = Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*"));
        await transaction.CommitAsync(CancellationToken.None);
        Directory.Delete(stage, recursive: true);
        Directory.SetLastWriteTimeUtc(selected, modified.AddDays(1));
        transaction.Dispose();
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "target")));
        Assert.Equal(modified, Directory.GetLastWriteTimeUtc(selected));
        Assert.Equal(new[] { Path.Combine(selected, "target") }, Directory.GetFileSystemEntries(selected));
    }

    private sealed class FailingAcknowledgements : MemoryStream {
        internal int Writes { get; private set; }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            if (++Writes == 4) { throw new IOException("Acknowledgement transport failed"); }
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }

    public void Dispose() { Directory.Delete(_parent, recursive: true); }
}
