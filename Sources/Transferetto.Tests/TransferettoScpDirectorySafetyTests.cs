using System.Text;

namespace Transferetto.Tests;

public sealed class TransferettoScpDirectorySafetyTests {
    [Theory]
    [InlineData("E\nC0644 1 outside.txt\nx\0")]
    [InlineData("C0644 1 outside.txt\nx\0")]
    [InlineData("D0755 0 root\nD0755 0 ..\n")]
    public async Task InvalidDirectoryTransitionsCannotWriteOutsideTheSelectedRoot(string wire) {
        await WithRoot(async root => {
            string selected = Path.Combine(root, "selected");
            Directory.CreateDirectory(selected);
            string sentinel = Path.Combine(root, "outside.txt");
            File.WriteAllText(sentinel, "original");
            using MemoryStream records = new(Encoding.UTF8.GetBytes(wire));
            using MemoryStream acknowledgements = new();
            ScpDirectoryReceiver receiver = new(records, acknowledgements, selected);
            await Assert.ThrowsAsync<InvalidDataException>(() => receiver.ReceiveAsync(CancellationToken.None));
            Assert.Equal("original", File.ReadAllText(sentinel));
            Assert.Empty(Directory.GetFileSystemEntries(selected));
        });
    }

    [Fact]
    public async Task NestedDirectoryRecordsRoundTripBinaryFilesAndEmptyDirectories() {
        await WithRoot(async root => {
            using MemoryStream records = new();
            byte[] start = Encoding.UTF8.GetBytes("D0755 0 root\nD0755 0 child\nC0644 3 file.bin\n");
            records.Write(start, 0, start.Length);
            records.Write(new byte[] { 0, 10, 255, 0 }, 0, 4);
            byte[] end = Encoding.UTF8.GetBytes("E\nD0755 0 empty\nE\nE\n");
            records.Write(end, 0, end.Length);
            records.Position = 0;
            using MemoryStream acknowledgements = new();
            ScpDirectoryReceiver receiver = new(records, acknowledgements, root);
            await receiver.ReceiveAsync(CancellationToken.None);
            Assert.Equal(new byte[] { 0, 10, 255 }, File.ReadAllBytes(Path.Combine(root, "child", "file.bin")));
            Assert.True(Directory.Exists(Path.Combine(root, "empty")));
        });
    }

    [Theory]
    [InlineData("D0755 0 root\nC0644 3 target\nxy")]
    [InlineData("D0755 0 root\nC0644 3 target\nxyz\u0001")]
    public async Task IncompleteOrUnconfirmedFileLeavesThePreviousDestinationIntact(string wire) {
        await WithRoot(async root => {
            string target = Path.Combine(root, "target");
            File.WriteAllText(target, "original");
            using MemoryStream records = new(Encoding.UTF8.GetBytes(wire));
            using MemoryStream acknowledgements = new();
            ScpDirectoryReceiver receiver = new(records, acknowledgements, root);
            await Assert.ThrowsAnyAsync<IOException>(() => receiver.ReceiveAsync(CancellationToken.None));
            Assert.Equal("original", File.ReadAllText(target));
            Assert.Single(Directory.GetFiles(root));
        });
    }

    private static async Task WithRoot(Func<string, Task> action) {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try { await action(root); }
        finally { Directory.Delete(root, recursive: true); }
    }
}
