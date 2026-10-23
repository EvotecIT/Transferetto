using FluentFTP;

namespace Transferetto.Tests;

public sealed class NativeFtpDirectoryTests {
    [Theory]
    [InlineData(FtpLocalExists.Skip, "old", "old")]
    [InlineData(FtpLocalExists.Resume, "ne", "new")]
    [InlineData(FtpLocalExists.Overwrite, "old", "new")]
    public void NativeFtpDirectoryPreservesSkipResumeAndOverwrite(FtpLocalExists mode, string original, string expected) {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "first"), original);
        try {
            using LoopbackFtpServer server = new(failSecond: false);
            using FtpClient client = new("127.0.0.1", "synthetic", "synthetic", server.Port);
            client.Config.ConnectTimeout = client.Config.ReadTimeout = 5000;
            client.Config.DataConnectionConnectTimeout = client.Config.DataConnectionReadTimeout = 5000;
            client.Config.RetryAttempts = 1;
            client.Connect();
            using TransferettoFtpSession session = new(client);
            var results = TransferettoClient.DownloadFtpDirectory(session, root, "/root", FtpFolderSyncMode.Update,
                mode, FtpVerify.None, null, null);
            Assert.DoesNotContain(results, result => result.IsFailed);
            Assert.Equal(expected, File.ReadAllText(Path.Combine(root, "first")));
            Assert.Equal("new", File.ReadAllText(Path.Combine(root, "second")));
            Assert.Equal(2, Directory.GetFileSystemEntries(root).Length);
            if (mode == FtpLocalExists.Resume) { Assert.Contains("REST 2", server.Commands); }
            if (mode == FtpLocalExists.Skip) { Assert.DoesNotContain("RETR /root/first", server.Commands); }
        } finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void NativeFtpFailureDoesNotOverwriteOrDeleteExistingMirrorFiles() {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "first"), "original");
        File.WriteAllText(Path.Combine(root, "unrelated"), "keep");
        try {
            using LoopbackFtpServer server = new();
            using FtpClient client = new("127.0.0.1", "synthetic", "synthetic", server.Port);
            client.Config.ConnectTimeout = 5000;
            client.Config.ReadTimeout = 5000;
            client.Config.DataConnectionConnectTimeout = 5000;
            client.Config.DataConnectionReadTimeout = 5000;
            client.Config.RetryAttempts = 1;
            client.Connect();
            using TransferettoFtpSession session = new(client);
            var results = TransferettoClient.DownloadFtpDirectory(session, root, "/root", FtpFolderSyncMode.Mirror,
                FtpLocalExists.Overwrite, FtpVerify.None, null, null);
            Assert.Equal(new[] { "first:original", "unrelated:keep" }, Directory.GetFiles(root)
                .Select(path => Path.GetFileName(path) + ":" + File.ReadAllText(path)).OrderBy(value => value, StringComparer.Ordinal));
            Assert.Contains(results, result => result.IsFailed);
            Assert.DoesNotContain(results, result => result.IsSuccess && !result.IsSkipped);
            Assert.Equal(2, Directory.GetFileSystemEntries(root).Length);
            Assert.Contains("RETR /root/first", server.Commands);
            Assert.Contains("RETR /root/second", server.Commands);
        } finally { Directory.Delete(root, recursive: true); }
    }
}
