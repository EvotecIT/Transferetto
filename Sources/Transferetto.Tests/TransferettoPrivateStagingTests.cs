using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoPrivateStagingTests : IDisposable {
    private readonly string _root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
    public TransferettoPrivateStagingTests() { Directory.CreateDirectory(_root); }

    [Fact]
    public void PrivateDirectoryIsExclusiveAndExistingDataIsNeverReusedOrRemoved() {
        string stage = Path.Combine(_root, "stage");
        TransferFileSystem.CreatePrivateStagingDirectory(stage);
        File.WriteAllText(Path.Combine(stage, "owned"), "preserve");
        Assert.Throws<IOException>(() => TransferFileSystem.CreatePrivateStagingDirectory(stage));
        Assert.Equal("preserve", File.ReadAllText(Path.Combine(stage, "owned")));
        Assert.Single(Directory.GetFileSystemEntries(stage));
#if NET8_0_OR_GREATER
        if (!OperatingSystem.IsWindows()) {
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(stage));
        }
#endif
    }

    [Fact]
    public void MissingParentDoesNotCreateAnUnprotectedDirectoryHierarchy() {
        Assert.Throws<IOException>(() => TransferFileSystem.CreatePrivateStagingDirectory(Path.Combine(_root, "missing", "stage")));
        Assert.Empty(Directory.GetFileSystemEntries(_root));
    }

    [Fact]
    public void ExistingFileCannotBecomeAStagingDirectory() {
        string target = Path.Combine(_root, "file");
        File.WriteAllText(target, "preserve");
        Assert.Throws<IOException>(() => TransferFileSystem.CreatePrivateStagingDirectory(target));
        Assert.Equal("preserve", File.ReadAllText(target));
        Assert.Equal(new[] { target }, Directory.GetFileSystemEntries(_root));
    }

    public void Dispose() { Directory.Delete(_root, recursive: true); }
}
