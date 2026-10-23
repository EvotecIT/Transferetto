using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class WindowsDirectoryCaseTests {
    [CaseSensitiveDirectoryFact]
    public async Task AutomaticSyncUsesTheSelectedCaseSensitiveDirectoryInsteadOfItsEndpointRoot() {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        string selected = Path.Combine(root, "selected");
        string source = Path.Combine(root, "source");
        Directory.CreateDirectory(selected);
        Directory.CreateDirectory(source);
        try {
            SetCaseSensitive(selected);
            SetCaseSensitive(source);
            File.WriteAllText(Path.Combine(source, "a.txt"), "lower");
            File.WriteAllText(Path.Combine(source, "A.txt"), "upper");
            FileSystemTransferEndpoint destination = new(root);
            Assert.Equal(StringComparison.OrdinalIgnoreCase, destination.GetPathComparison());
            Assert.Equal(StringComparison.Ordinal, destination.GetPathComparison("selected"));
            TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(
                new FileSystemTransferEndpoint(source), "", destination, "selected");
            Assert.True(result.IsSuccess);
            Assert.Equal("lower", File.ReadAllText(Path.Combine(selected, "a.txt")));
            Assert.Equal("upper", File.ReadAllText(Path.Combine(selected, "A.txt")));
            Assert.Equal(2, Directory.GetFiles(selected).Length);
        } finally { Directory.Delete(root, recursive: true); }
    }

    private static void SetCaseSensitive(string path) {
        Assert.True(TrySetCaseSensitive(path), "Setting case sensitivity on the disposable directory failed: " + Marshal.GetLastWin32Error());
    }

    private static bool TrySetCaseSensitive(string path) {
        using SafeFileHandle handle = OpenDirectory(path, 0x100, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (handle.IsInvalid) { return false; }
        uint flags = 1;
        return SetInformation(handle, 23, ref flags, sizeof(uint));
    }

    private sealed class CaseSensitiveDirectoryFactAttribute : FactAttribute {
        public CaseSensitiveDirectoryFactAttribute() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { Skip = "Requires Windows per-directory case settings."; return; }
            string probe = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(probe);
            try {
                if (!TrySetCaseSensitive(probe)) {
                    Skip = "This Windows identity cannot enable case-sensitive directories (error " + Marshal.GetLastWin32Error() + "). No elevation is requested.";
                }
            } finally { Directory.Delete(probe); }
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle OpenDirectory(string path, uint access, uint share, IntPtr security,
        uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformation(SafeFileHandle handle, int informationClass, ref uint flags, uint size);
}
