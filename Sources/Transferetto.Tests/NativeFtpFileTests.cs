using FluentFTP;
using FluentFTP.Exceptions;

namespace Transferetto.Tests;

public sealed class NativeFtpFileTests : IDisposable {
    private readonly string _parent = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
    public NativeFtpFileTests() { Directory.CreateDirectory(_parent); }

    [Theory]
    [InlineData(FtpLocalExists.Overwrite, "original")]
    [InlineData(FtpLocalExists.Resume, "n")]
    public void ShortSuccessfulFtpFilePreservesTheOriginal(FtpLocalExists mode, string original) {
        string target = Path.Combine(_parent, "second");
        File.WriteAllText(target, original);
        using LoopbackFtpServer server = new(failSecond: false, shortSecond: true, shortContent: "ne");
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        var result = TransferettoClient.DownloadFtpFile(session, target, "/root/second", mode);
        Assert.Equal(original, File.ReadAllText(target));
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        Assert.Single(Directory.GetFileSystemEntries(_parent));
        if (mode == FtpLocalExists.Resume) { Assert.Contains("REST 1", server.Commands); }
    }

    [Fact]
    public void FailedFtpFileDoesNotCreateItsDestinationTree() {
        string root = Path.Combine(_parent, "new");
        using LoopbackFtpServer server = new(failSecond: false, shortSecond: true);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        var result = TransferettoClient.DownloadFtpFile(session, Path.Combine(root, "second"), "/root/second", FtpLocalExists.Overwrite);
        Assert.False(Directory.Exists(root));
        Assert.True(result.IsFailed);
        Assert.Empty(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(FtpError.None, true)]
    [InlineData(FtpError.Stop, true)]
    [InlineData(FtpError.Throw, true)]
    [InlineData(FtpError.DeleteProcessed, true)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Stop, true)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Throw, true)]
    [InlineData(FtpError.None, false)]
    [InlineData(FtpError.Stop, false)]
    [InlineData(FtpError.Throw, false)]
    [InlineData(FtpError.DeleteProcessed, false)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Stop, false)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Throw, false)]
    public void FtpBatchFailureHonorsItsErrorPolicyWithoutLosingOriginals(FtpError policy, bool shortContent) {
        File.WriteAllText(Path.Combine(_parent, "first"), "original-first");
        File.WriteAllText(Path.Combine(_parent, "second"), "original-second");
        using LoopbackFtpServer server = new(failSecond: !shortContent, shortSecond: shortContent);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        IReadOnlyList<TransferettoTransferResult>? results = null;
        void Receive() => results = TransferettoClient.DownloadFtpFiles(session, _parent,
            new[] { "/root/first", "/root/second", "/root/third" }, FtpLocalExists.Overwrite, FtpVerify.None, policy);
        if (policy.HasFlag(FtpError.Throw)) { Assert.Throws<FtpException>(Receive); }
        else { Receive(); Assert.Contains(results!, result => result.RemotePath == "/root/second" && result.IsFailed); }
        Assert.Equal("original-second", File.ReadAllText(Path.Combine(_parent, "second")));
        bool rollback = policy.HasFlag(FtpError.DeleteProcessed);
        Assert.Equal(rollback ? "original-first" : "new", File.ReadAllText(Path.Combine(_parent, "first")));
        bool continues = !policy.HasFlag(FtpError.Stop) && !policy.HasFlag(FtpError.Throw);
        Assert.Equal(continues, server.Commands.Contains("RETR /root/third"));
        Assert.Equal(continues && !rollback, File.Exists(Path.Combine(_parent, "third")));
        if (continues && !rollback) { Assert.Equal("new", File.ReadAllText(Path.Combine(_parent, "third"))); }
        if (rollback && results != null) { Assert.DoesNotContain(results, result => result.IsSuccess && !result.IsSkipped); }
        Assert.DoesNotContain(Directory.GetFileSystemEntries(_parent), path => Path.GetFileName(path).StartsWith(".transferetto-", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, FtpLocalExists.Overwrite, "old", "new")]
    [InlineData(false, FtpLocalExists.Resume, "ne", "new")]
    [InlineData(false, FtpLocalExists.Resume, "new", "new")]
    [InlineData(false, FtpLocalExists.Skip, "old", "old")]
    [InlineData(true, FtpLocalExists.Overwrite, "old", "new")]
    [InlineData(true, FtpLocalExists.Resume, "ne", "new")]
    [InlineData(true, FtpLocalExists.Resume, "new", "new")]
    [InlineData(true, FtpLocalExists.Skip, "old", "old")]
    public void FtpFileAndRollbackBatchRetainTransferModesAndLogicalProgress(bool batch, FtpLocalExists mode, string original, string expected) {
        string target = Path.Combine(_parent, "first");
        File.WriteAllText(target, original);
        List<TransferettoTransferProgress> events = new();
        using LoopbackFtpServer server = new(failSecond: false);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        TransferettoTransferOptions options = new() { Progress = new InlineProgress(events.Add) };
        var result = batch ? TransferettoClient.DownloadFtpFiles(session, _parent, new[] { "/root/first" }, mode,
            FtpVerify.None, FtpError.DeleteProcessed | FtpError.Stop, options).Single()
            : TransferettoClient.DownloadFtpFile(session, target, "/root/first", mode, FtpVerify.None, options);
        Assert.False(result.IsFailed);
        Assert.Equal(expected, File.ReadAllText(target));
        Assert.Equal(target, result.LocalPath);
        Assert.Single(Directory.GetFileSystemEntries(_parent));
        bool skipped = mode == FtpLocalExists.Skip || (mode == FtpLocalExists.Resume && original == "new");
        Assert.Equal(skipped, result.IsSkipped);
        Assert.Equal(!skipped, server.Commands.Contains("RETR /root/first"));
        Assert.All(events, progress => { Assert.Equal(target, progress.LocalPath); Assert.Equal("/root/first", progress.RemotePath); });
        if (!skipped) { Assert.NotEmpty(events); }
        if (mode == FtpLocalExists.Resume && !skipped) { Assert.Contains("REST 2", server.Commands); }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public void DisabledZeroByteFilesAreSkippedEvenWithoutASizeCommand(bool batch, bool knownSize) {
        string target = Path.Combine(_parent, "second");
        File.WriteAllText(target, "keep");
        using LoopbackFtpServer server = new(failSecond: false, emptySecond: true, knownSize: knownSize);
        using FtpClient client = Connect(server);
        client.Config.DownloadZeroByteFiles = false;
        using TransferettoFtpSession session = new(client);
        var result = batch ? TransferettoClient.DownloadFtpFiles(session, _parent, new[] { "/root/second" },
            FtpLocalExists.Overwrite, FtpVerify.None, FtpError.DeleteProcessed | FtpError.Stop).Single()
            : TransferettoClient.DownloadFtpFile(session, target, "/root/second", FtpLocalExists.Overwrite);
        Assert.True(result.IsSkipped);
        Assert.False(result.IsFailed);
        Assert.Equal("keep", File.ReadAllText(target));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FtpCompletionAcceptsUnknownSizesAndAsciiTransfers(bool ascii) {
        using LoopbackFtpServer server = new(failSecond: false, shortSecond: true, knownSize: ascii);
        using FtpClient client = Connect(server);
        if (ascii) { client.Config.DownloadDataType = FtpDataType.ASCII; }
        using TransferettoFtpSession session = new(client);
        string target = Path.Combine(_parent, "second");
        var result = TransferettoClient.DownloadFtpFile(session, target, "/root/second", FtpLocalExists.Overwrite);
        Assert.True(result.IsSuccess);
        Assert.Equal("n", File.ReadAllText(target));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FtpBatchCancellationKeepsTheSelectedRollbackPolicy(bool rollback) {
        File.WriteAllText(Path.Combine(_parent, "first"), "original-first");
        File.WriteAllText(Path.Combine(_parent, "second"), "original-second");
        using CancellationTokenSource cancellation = new();
        using LoopbackFtpServer server = new(failSecond: false);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        TransferettoTransferOptions options = new() { CancellationToken = cancellation.Token,
            Progress = new InlineProgress(progress => { if (progress.RemotePath == "/root/second") { cancellation.Cancel(); } }) };
        Assert.ThrowsAny<OperationCanceledException>(() => TransferettoClient.DownloadFtpFiles(session, _parent,
            new[] { "/root/first", "/root/second" }, FtpLocalExists.Overwrite, FtpVerify.None,
            rollback ? FtpError.DeleteProcessed : FtpError.None, options));
        Assert.Equal(rollback ? "original-first" : "new", File.ReadAllText(Path.Combine(_parent, "first")));
        Assert.Equal("original-second", File.ReadAllText(Path.Combine(_parent, "second")));
        Assert.Equal(2, Directory.GetFileSystemEntries(_parent).Length);
    }

    [Fact]
    public void FtpMirrorSyncStopsBeforeDeletesAfterAShortFile() {
        File.WriteAllText(Path.Combine(_parent, "first"), "original-first");
        File.WriteAllText(Path.Combine(_parent, "second"), "original-second");
        File.WriteAllText(Path.Combine(_parent, "unrelated"), "keep");
        using LoopbackFtpServer server = new(failSecond: false, shortSecond: true);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        var results = TransferettoClient.SyncFtpDirectory(session, _parent, "/root", new TransferettoSyncOptions {
            Direction = TransferettoSyncDirection.Download, Mode = TransferettoSyncMode.Mirror, PreserveTimestamps = false
        });
        Assert.Contains(results, result => !result.Status && !result.IsSkipped);
        Assert.Equal("new", File.ReadAllText(Path.Combine(_parent, "first")));
        Assert.Equal("original-second", File.ReadAllText(Path.Combine(_parent, "second")));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(_parent, "unrelated")));
        Assert.Equal(3, Directory.GetFileSystemEntries(_parent).Length);
    }

    [Theory]
    [InlineData("/other/first", typeof(InvalidDataException))]
    [InlineData("/root/..", typeof(ArgumentException))]
    public void FtpBatchRejectsInvalidOrCollidingDestinationsBeforeReceiving(string second, Type exceptionType) {
        string target = Path.Combine(_parent, "first");
        File.WriteAllText(target, "original");
        using LoopbackFtpServer server = new(failSecond: false);
        using FtpClient client = Connect(server);
        using TransferettoFtpSession session = new(client);
        Assert.Throws(exceptionType, () => TransferettoClient.DownloadFtpFiles(session, _parent,
            new[] { "/root/first", second }, FtpLocalExists.Overwrite));
        Assert.DoesNotContain(server.Commands, command => command.StartsWith("RETR ", StringComparison.Ordinal));
        Assert.Equal("original", File.ReadAllText(target));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData("file", false)]
    [InlineData("file", true)]
    [InlineData("batch", true)]
    [InlineData("directory", true)]
    public void AsciiExpansionIsReadToEndAcrossFtpDownloadSurfaces(string surface, bool reportProgress) {
        string target = Path.Combine(_parent, "second");
        File.WriteAllText(target, "original");
        List<TransferettoTransferProgress> events = new();
        using LoopbackFtpServer server = new(failSecond: false, asciiExpansion: true);
        using FtpClient client = Connect(server);
        client.Config.DownloadDataType = FtpDataType.ASCII;
        using TransferettoFtpSession session = new(client);
        TransferettoTransferOptions? options = reportProgress ? new() { Progress = new InlineProgress(events.Add) } : null;
        var results = surface == "directory" ? TransferettoClient.DownloadFtpDirectory(session, _parent, "/root",
            FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, options)
            : surface == "batch" ? TransferettoClient.DownloadFtpFiles(session, _parent, new[] { "/root/second" },
                FtpLocalExists.Overwrite, FtpVerify.None, FtpError.DeleteProcessed | FtpError.Stop, options)
            : new[] { TransferettoClient.DownloadFtpFile(session, target, "/root/second", FtpLocalExists.Overwrite, FtpVerify.None, options) };
        Assert.Equal("a\r\nb\r\n", File.ReadAllText(target));
        Assert.DoesNotContain(results, result => result.IsFailed);
        if (reportProgress) { Assert.Contains(events, progress => progress.LocalPath == target && progress.BytesTransferred == 6); }
        Assert.Contains("TYPE A", server.Commands);
    }

    [Theory]
    [InlineData(FtpError.DeleteProcessed)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Stop)]
    [InlineData(FtpError.DeleteProcessed | FtpError.Throw)]
    public void FtpBatchPublicationFailureHonorsTheSelectedErrorPolicy(FtpError policy) {
        string first = Path.Combine(_parent, "first"), second = Path.Combine(_parent, "second");
        File.WriteAllText(first, "original-first"); File.WriteAllText(second, "original-second");
        File.SetAttributes(second, FileAttributes.ReadOnly);
        try {
            using LoopbackFtpServer server = new(failSecond: false);
            using FtpClient client = Connect(server);
            using TransferettoFtpSession session = new(client);
            IReadOnlyList<TransferettoTransferResult>? results = null;
            void Receive() => results = TransferettoClient.DownloadFtpFiles(session, _parent,
                new[] { "/root/first", "/root/second" }, FtpLocalExists.Overwrite, FtpVerify.None, policy);
            if (policy.HasFlag(FtpError.Throw)) { Assert.IsType<UnauthorizedAccessException>(Assert.Throws<FtpException>(Receive).InnerException); }
            else { Receive(); Assert.Equal(2, results!.Count); Assert.All(results, result => { Assert.True(result.IsFailed); Assert.False(result.IsSuccess); }); }
            Assert.Equal("original-first", File.ReadAllText(first));
            Assert.Equal("original-second", File.ReadAllText(second));
            Assert.Equal(2, Directory.GetFileSystemEntries(_parent).Length);
        } finally { File.SetAttributes(second, FileAttributes.Normal); }
    }

    [Theory]
    [InlineData("file")]
    [InlineData("batch")]
    [InlineData("directory")]
    public void AsciiCancellationAfterReceiptWithholdsPublication(string surface) {
        string target = Path.Combine(_parent, "second");
        File.WriteAllText(target, "original");
        using CancellationTokenSource cancellation = new();
        using LoopbackFtpServer server = new(failSecond: false, asciiExpansion: true);
        using FtpClient client = Connect(server);
        client.Config.DownloadDataType = FtpDataType.ASCII;
        using TransferettoFtpSession session = new(client);
        TransferettoTransferOptions options = new() { CancellationToken = cancellation.Token,
            Progress = new InlineProgress(progress => { if (progress.RemotePath == "/root/second") { cancellation.Cancel(); } }) };
        Assert.ThrowsAny<OperationCanceledException>(() => {
            if (surface == "file") { TransferettoClient.DownloadFtpFile(session, target, "/root/second", FtpLocalExists.Overwrite, FtpVerify.None, options); }
            else if (surface == "batch") { TransferettoClient.DownloadFtpFiles(session, _parent, new[] { "/root/second" }, FtpLocalExists.Overwrite,
                FtpVerify.None, FtpError.DeleteProcessed, options); }
            else { TransferettoClient.DownloadFtpDirectory(session, _parent, "/root", FtpFolderSyncMode.Update,
                FtpLocalExists.Overwrite, FtpVerify.None, null, options); }
        });
        Assert.Equal("original", File.ReadAllText(target));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
    }

    [Theory]
    [InlineData("file")]
    [InlineData("batch")]
    [InlineData("directory")]
    public void NativeChecksumRetryDeleteAndThrowNeverTouchTheOriginal(string surface) {
        string target = Path.Combine(_parent, "second");
        File.WriteAllText(target, "original");
        using LoopbackFtpServer server = new(failSecond: false, badChecksum: true);
        using FtpClient client = Connect(server);
        client.Config.RetryAttempts = 2;
        Assert.True(client.HasFeature(FtpCapability.XMD5), string.Join(";", server.Commands));
        using TransferettoFtpSession session = new(client);
        FtpVerify verify = FtpVerify.Retry | FtpVerify.Delete | FtpVerify.Throw;
        if (surface == "file") {
            Assert.Throws<FtpException>(() => TransferettoClient.DownloadFtpFile(session, target, "/root/second", FtpLocalExists.Overwrite, verify));
        } else {
            var results = surface == "batch" ? TransferettoClient.DownloadFtpFiles(session, _parent, new[] { "/root/second" },
                FtpLocalExists.Overwrite, verify, FtpError.Stop)
                : TransferettoClient.DownloadFtpDirectory(session, _parent, "/root", FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, verify, null, null);
            Assert.Contains(results, result => result.IsFailed && result.RemotePath == "/root/second");
        }
        Assert.Equal(2, server.Commands.Count(command => command == "RETR /root/second"));
        Assert.Equal(2, server.Commands.Count(command => command == "XMD5 /root/second"));
        Assert.Equal("original", File.ReadAllText(target));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task FileOnlyTransactionDoesNotReportAFalseCleanupFailure() {
        string scheme = "test-" + Guid.NewGuid().ToString("N");
        long failures = 0;
        using System.Diagnostics.Metrics.MeterListener listener = new();
        listener.InstrumentPublished = (instrument, owner) => {
            if (instrument.Meter.Name == Transferetto.Core.TransferDiagnostics.Name && instrument.Name == "transferetto.cleanup.failures") {
                owner.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, state) => {
            foreach (var tag in tags) { if (tag.Key == "transfer.scheme" && Equals(tag.Value, scheme)) { Interlocked.Add(ref failures, value); } }
        });
        listener.Start();
        using (LocalDirectoryTransaction transaction = new(_parent, scheme)) {
            transaction.AddFile("file", staged => { File.WriteAllText(staged, "complete"); return true; });
            await transaction.CommitAsync(CancellationToken.None);
        }
        Assert.Equal("complete", File.ReadAllText(Path.Combine(_parent, "file")));
        Assert.Single(Directory.GetFileSystemEntries(_parent));
        Assert.Equal(0, Interlocked.Read(ref failures));
    }
#endif

    private sealed class InlineProgress : IProgress<TransferettoTransferProgress> {
        private readonly Action<TransferettoTransferProgress> _report;
        internal InlineProgress(Action<TransferettoTransferProgress> report) { _report = report; }
        public void Report(TransferettoTransferProgress value) => _report(value);
    }

    private static FtpClient Connect(LoopbackFtpServer server) {
        FtpClient client = new("127.0.0.1", "synthetic", "synthetic", server.Port);
        client.Config.ConnectTimeout = client.Config.ReadTimeout = 5000;
        client.Config.DataConnectionConnectTimeout = client.Config.DataConnectionReadTimeout = 5000;
        client.Config.RetryAttempts = 1;
        client.Connect();
        return client;
    }

    public void Dispose() { Directory.Delete(_parent, recursive: true); }
}
