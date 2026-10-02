using System.Reflection;
using System.Runtime.InteropServices;
using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoLocalSafetyTests {
    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("folder/../../outside.txt")]
    [InlineData("/outside.txt")]
    [InlineData("folder/./file.txt")]
    public void RemoteNamesCannotEscapeOrAliasTheLocalRoot(string path) {
        Assert.Throws<ArgumentException>(() => TransferFileSystem.ResolveRelativePath(Path.GetTempPath(), path));
    }

    [Theory]
    [InlineData("..\\outside.txt")]
    [InlineData("C:\\outside.txt")]
    [InlineData("\\outside.txt")]
    [InlineData("file.txt:stream")]
    [InlineData("CON.txt")]
    [InlineData("file.txt.")]
    public void WindowsRejectsUnrepresentableRemoteNamesWhileUnixPreservesLiteralBackslashes(string path) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Assert.Throws<ArgumentException>(() => TransferFileSystem.ResolveRelativePath(Path.GetTempPath(), path));
        } else {
            Assert.Equal(Path.Combine(Path.GetTempPath(), path), TransferFileSystem.ResolveRelativePath(Path.GetTempPath(), path));
        }
    }

    [Fact]
    public void AtomicCommitFailurePreservesTheOldDestinationAndStaging() {
        WithDirectory(root => {
            string target = Path.Combine(root, "target.txt");
            string staged = Path.Combine(root, "staged.txt");
            File.WriteAllText(target, "original"); File.WriteAllText(staged, "replacement");
            Assert.Throws<IOException>(() => TransferFileSystem.CommitStagedFile(staged, target, overwrite: false));
            Assert.Equal("original", File.ReadAllText(target)); Assert.Equal("replacement", File.ReadAllText(staged));
            TransferFileSystem.CommitStagedFile(staged, target);
            Assert.Equal("replacement", File.ReadAllText(target)); Assert.False(File.Exists(staged));
        });
    }

    [Fact]
    public void ProtocolAtomicWriterReplacesExistingFiles() {
        WithDirectory(root => {
            string target = Path.Combine(root, "target.txt"); File.WriteAllText(target, "original");
            Action<FileStream> writer = stream => { byte[] data = System.Text.Encoding.UTF8.GetBytes("replacement"); stream.Write(data, 0, data.Length); };
            typeof(TransferettoClient).GetMethod("WriteLocalFileAtomically", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, new object[] { target, writer });
            Assert.Equal("replacement", File.ReadAllText(target)); Assert.Single(Directory.GetFiles(root));
        });
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void LinkedDirectoryManifestFailsBeforeExposingOutsideFiles() {
        WithDirectory(root => {
            string outside = Path.Combine(root, "outside"); string selected = Path.Combine(root, "selected");
            Directory.CreateDirectory(outside); Directory.CreateDirectory(selected);
            string sentinel = Path.Combine(outside, "private.txt"); File.WriteAllText(sentinel, "private");
            string link = Path.Combine(selected, "linked");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                System.Diagnostics.ProcessStartInfo start = new("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (string argument in new[] { "/c", "mklink", "/J", link, outside }) { start.ArgumentList.Add(argument); }
                using System.Diagnostics.Process process = System.Diagnostics.Process.Start(start)!;
                process.WaitForExit();
                Assert.Equal(0, process.ExitCode);
            } else {
                Directory.CreateSymbolicLink(link, outside);
            }
            try {
                TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => typeof(TransferettoClient)
                    .GetMethod("BuildLocalSyncManifest", BindingFlags.Static | BindingFlags.NonPublic)!
                    .Invoke(null, new object[] { selected, "/remote", CancellationToken.None }));
                Assert.IsType<IOException>(error.InnerException); Assert.Equal("private", File.ReadAllText(sentinel));
            } finally {
                Directory.Delete(link);
            }
        });
    }
#endif

    private static void WithDirectory(Action<string> action) {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try { action(root); } finally { Directory.Delete(root, recursive: true); }
    }
}
