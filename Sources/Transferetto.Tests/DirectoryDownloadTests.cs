using FluentFTP;
using FluentFTP.Rules;

namespace Transferetto.Tests;

public sealed class DirectoryDownloadTests : IDisposable {
    private readonly string _parent = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
    public DirectoryDownloadTests() { Directory.CreateDirectory(_parent); }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedFtpDirectoryDoesNotPublishEarlierFilesOrDeleteMirrorExtras(bool existing) {
        string selected = Path.Combine(_parent, "download");
        if (existing) {
            Directory.CreateDirectory(selected);
            File.WriteAllText(Path.Combine(selected, "first"), "original");
            File.WriteAllText(Path.Combine(selected, "unrelated"), "keep");
        }
        DirectoryFtpClient client = new() { FailSecond = true };

        IReadOnlyList<TransferettoTransferResult> results = TransferettoClient.DownloadFtpDirectoryCore(client,
            selected, "/root", FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite, FtpVerify.None, null, new TransferettoTransferOptions());
        Assert.Contains(results, result => result.IsFailed);
        Assert.DoesNotContain(results, result => result.IsSuccess && !result.IsSkipped);
        if (existing) {
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "first")));
            Assert.Equal("keep", File.ReadAllText(Path.Combine(selected, "unrelated")));
            Assert.Equal(2, Directory.GetFileSystemEntries(selected).Length);
        } else { Assert.False(Directory.Exists(selected)); }
        Assert.Equal(existing ? new[] { selected } : Array.Empty<string>(), Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedSftpDirectoryPreservesOriginalsAndDoesNotLeaveANewTree(bool existing) {
        string selected = Path.Combine(_parent, "download");
        if (existing) {
            Directory.CreateDirectory(selected);
            File.WriteAllText(Path.Combine(selected, "first"), "original");
        }
        int reads = 0;
        TransferettoTransferResult Receive(string remote, string staged, string logical, TransferettoTransferOptions options) {
            if (++reads == 2) { throw new EndOfStreamException("The second file was truncated."); }
            File.WriteAllText(staged, "new");
            return new TransferettoTransferResult { Status = true, IsSuccess = true, LocalPath = logical, RemotePath = remote };
        }
        Assert.Throws<EndOfStreamException>(() => TransferettoClient.DownloadSftpDirectoryCore(SftpListing, Receive,
            "/root", selected, true, new TransferettoTransferOptions()));
        Assert.Equal(2, reads);
        if (existing) {
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "first")));
            Assert.Single(Directory.GetFileSystemEntries(selected));
        } else { Assert.False(Directory.Exists(selected)); }
        Assert.Equal(existing ? new[] { selected } : Array.Empty<string>(), Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationAfterReceiptDoesNotPublishEitherProtocol(bool ftp) {
        string selected = Path.Combine(_parent, "download");
        using CancellationTokenSource cancellation = new();
        TransferettoTransferOptions options = new() { CancellationToken = cancellation.Token };
        void Receive() {
            if (ftp) {
                DirectoryFtpClient client = new() { AfterFirst = cancellation.Cancel };
                TransferettoClient.DownloadFtpDirectoryCore(client, selected, "/root", FtpFolderSyncMode.Update,
                    FtpLocalExists.Overwrite, FtpVerify.None, null, options);
            } else {
                TransferettoClient.DownloadSftpDirectoryCore(SftpListing, (remote, staged, logical, transfer) => {
                    File.WriteAllText(staged, "new"); cancellation.Cancel();
                    return new TransferettoTransferResult { Status = true, IsSuccess = true };
                }, "/root", selected, true, options);
            }
        }
        Assert.ThrowsAny<OperationCanceledException>(Receive);
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(FtpLocalExists.Overwrite, "old", "new")]
    [InlineData(FtpLocalExists.Resume, "ne", "new")]
    [InlineData(FtpLocalExists.Skip, "old", "old")]
    public void SuccessfulFtpMirrorPreservesCollisionModesAndRemovesOnlyExtras(FtpLocalExists mode, string old, string expected) {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "first"), old);
        File.WriteAllText(Path.Combine(selected, "unrelated"), "remove");
        var results = TransferettoClient.DownloadFtpDirectoryCore(new DirectoryFtpClient(), selected, "/root",
            FtpFolderSyncMode.Mirror, mode, FtpVerify.None, null, new TransferettoTransferOptions());
        Assert.DoesNotContain(results, result => result.IsFailed);
        Assert.Equal(expected, File.ReadAllText(Path.Combine(selected, "first")));
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "second")));
        Assert.Equal(2, Directory.GetFileSystemEntries(selected).Length);
        if (mode == FtpLocalExists.Skip) { Assert.Contains(results, result => result.IsSkipped && result.LocalPath == Path.Combine(selected, "first")); }
    }

    [Fact]
    public void SftpUsesOneValidatedManifestAndRetainsSkippedFilesAndEmptyDirectories() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "first"), "original");
        List<string> listed = new();
        IReadOnlyList<TransferettoSftpItem> List(string path) {
            listed.Add(path);
            if (path.TrimEnd('/') != "/root") { return Array.Empty<TransferettoSftpItem>(); }
            return SftpListing(path).Concat(new[] { new TransferettoSftpItem { Name = "empty", IsDirectory = true } }).ToArray();
        }
        var results = TransferettoClient.DownloadSftpDirectoryCore(List, (remote, staged, logical, options) => {
            Assert.False(Directory.Exists(Path.Combine(selected, "empty")));
            Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "first")));
            File.WriteAllText(staged, "new");
            return new TransferettoTransferResult { Status = true, IsSuccess = true, LocalPath = logical, RemotePath = remote };
        }, "/root", selected, false, new TransferettoTransferOptions());
        Assert.Equal(new[] { "/root/", "/root/empty" }, listed);
        Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "first")));
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "second")));
        Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(selected, "empty")));
        Assert.Equal(3, Directory.GetFileSystemEntries(selected).Length);
        Assert.Contains(results, result => result.IsSkipped);
    }

    private static IReadOnlyList<TransferettoSftpItem> SftpListing(string path) => new[] {
        new TransferettoSftpItem { Name = "first", FullName = "/root/first", IsRegularFile = true, Length = 3 },
        new TransferettoSftpItem { Name = "second", FullName = "/root/second", IsRegularFile = true, Length = 3 }
    };

    [Theory]
    [InlineData(FtpDataType.Binary, -1)]
    [InlineData(FtpDataType.ASCII, 9)]
    public void UnknownOrAsciiRemoteSizesDoNotRejectACompletedFtpDirectory(FtpDataType dataType, long size) {
        string selected = Path.Combine(_parent, "download");
        DirectoryFtpClient client = new() { DataType = dataType, RemoteSize = size };
        var results = TransferettoClient.DownloadFtpDirectoryCore(client, selected, "/root", FtpFolderSyncMode.Update,
            FtpLocalExists.Overwrite, FtpVerify.None, null, new TransferettoTransferOptions());
        Assert.DoesNotContain(results, result => result.IsFailed);
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "first")));
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "second")));
    }

    [Theory]
    [InlineData(false, "../escape")]
    [InlineData(true, "../escape")]
    [InlineData(false, "first")]
    [InlineData(true, "first")]
    public void InvalidOrDuplicateManifestIsRejectedBeforeEitherProtocolReceivesContent(bool ftp, string name) {
        string selected = Path.Combine(_parent, "download");
        int received = 0;
        void Receive() {
            if (ftp) {
                DirectoryFtpClient client = new() { SecondName = name, AfterFirst = () => received++ };
                TransferettoClient.DownloadFtpDirectoryCore(client, selected, "/root", FtpFolderSyncMode.Update,
                    FtpLocalExists.Overwrite, FtpVerify.None, null, new TransferettoTransferOptions());
            } else {
                TransferettoClient.DownloadSftpDirectoryCore(_ => new[] {
                    new TransferettoSftpItem { Name = "first", IsRegularFile = true },
                    new TransferettoSftpItem { Name = name, IsRegularFile = true }
                }, (_, _, _, _) => { received++; throw new InvalidOperationException("Unexpected content read"); },
                    "/root", selected, true, new TransferettoTransferOptions());
            }
        }
        Assert.Throws<InvalidDataException>(Receive);
        Assert.Equal(0, received);
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FtpMirrorUsesNativeRulesAndTheConfiguredExcludedFilePolicy(bool deleteExcluded) {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "first"), "excluded");
        File.WriteAllText(Path.Combine(selected, "unrelated"), "excluded-extra");
        var results = TransferettoClient.DownloadFtpDirectoryCore(new DirectoryFtpClient { DeleteExcluded = deleteExcluded },
            selected, "/root", FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite, FtpVerify.None,
            new[] { new OnlySecondRule() }, new TransferettoTransferOptions());
        Assert.Contains(results, result => result.IsSkippedByRule && result.RemotePath == "/root/first");
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "second")));
        if (deleteExcluded) { Assert.Single(Directory.GetFileSystemEntries(selected)); }
        else {
            Assert.Equal("excluded", File.ReadAllText(Path.Combine(selected, "first")));
            Assert.Equal("excluded-extra", File.ReadAllText(Path.Combine(selected, "unrelated")));
            Assert.Equal(3, Directory.GetFileSystemEntries(selected).Length);
        }
    }

    [Fact]
    public void PublicationFailureRestoresAnEarlierMirrorDeletionAndReplacement() {
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "first"), "original");
        string extra = Path.Combine(selected, "extra");
        File.WriteAllText(extra, "restore");
        DateTime modified = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(extra, modified);
        using (LocalDirectoryTransaction transaction = new(selected, "ftp")) {
            transaction.AddDirectory("", null, null);
            transaction.AddFile("first", path => { File.WriteAllText(path, "new"); return true; });
            transaction.DeleteFile("extra");
            string? lastStage = null;
            transaction.AddFile("last", path => { lastStage = path; File.WriteAllText(path, "last"); return true; });
            // Real late I/O failure after the first replacement and mirror deletion.
            File.Delete(lastStage!);
            Assert.Throws<FileNotFoundException>(() => transaction.CommitAsync(CancellationToken.None).GetAwaiter().GetResult());
        }
        Assert.Equal("original", File.ReadAllText(Path.Combine(selected, "first")));
        Assert.Equal("restore", File.ReadAllText(extra));
        Assert.Equal(modified, File.GetLastWriteTimeUtc(extra));
        Assert.Equal(2, Directory.GetFileSystemEntries(selected).Length);
    }

    private sealed class OnlySecondRule : FtpRule {
        public override bool IsAllowed(FtpListItem item) => item.Name == "second";
    }

    private sealed class DirectoryFtpClient : IFtpDirectoryDownloadOperations {
        internal bool FailSecond { get; set; }
        internal Action? AfterFirst { get; set; }
        internal string SecondName { get; set; } = "second";
        public bool DeleteExcluded { get; set; }
        public bool DownloadZeroByteFiles => true;
        internal FtpDataType DataType { get; set; } = FtpDataType.Binary;
        internal long RemoteSize { get; set; } = 3;
        public FtpDataType DownloadDataType => DataType;
        public bool DirectoryExists(string path) => true;
        public IReadOnlyList<FtpListItem> ListDirectory(string path) => new[] {
            new FtpListItem { Name = "first", FullName = "/root/first", Type = FtpObjectType.File, Size = RemoteSize },
            new FtpListItem { Name = SecondName, FullName = "/root/" + SecondName, Type = FtpObjectType.File, Size = RemoteSize }
        };
        public FtpStatus DownloadFile(string localPath, string remotePath, FtpLocalExists existsMode,
            FtpVerify verifyOptions, Action<FtpProgress> progress) {
            bool failed = remotePath.EndsWith("second", StringComparison.Ordinal) && FailSecond;
            string content = failed ? "x" : "new";
            if (existsMode == FtpLocalExists.Skip && File.Exists(localPath)) { return FtpStatus.Skipped; }
            if (existsMode == FtpLocalExists.Resume && File.Exists(localPath)) {
                long length = new FileInfo(localPath).Length;
                if (length >= content.Length) { return FtpStatus.Skipped; }
                File.AppendAllText(localPath, content.Substring((int)length));
            }
            else { File.WriteAllText(localPath, content); }
            if (remotePath.EndsWith("first", StringComparison.Ordinal)) { AfterFirst?.Invoke(); }
            return failed ? FtpStatus.Failed : FtpStatus.Success;
        }
    }

    public void Dispose() { Directory.Delete(_parent, recursive: true); }
}
