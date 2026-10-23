using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Transferetto.Core;

public static partial class TransferFileSystem {
    /// <summary>Exclusively creates a staging directory with owner-only access, without inheriting broad parent permissions.</summary>
    /// <remarks>The parent must exist and be controlled by the caller. Existing directories are never reused.
    /// Windows requires a filesystem that persists protected access rules; Unix requests mode 0700 at creation.</remarks>
    public static void CreatePrivateStagingDirectory(string path) {
        string full = Path.GetFullPath(path);
        string parent = Path.GetDirectoryName(full) ?? throw new ArgumentException("A staging parent is required.", nameof(path));
        EnsureNoLinkTraversal(parent, full);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            if (CreateUnixPrivateDirectory(Encoding.UTF8.GetBytes(full + '\0'), 448) != 0) { throw NativeDirectoryError(); }
            try {
#if NET8_0_OR_GREATER
                int mode = (int)File.GetUnixFileMode(full);
#else
                if (GetUnixMode == null) { throw new PlatformNotSupportedException("Private staging requires Unix file-mode support."); }
                int mode = Convert.ToInt32(GetUnixMode.Invoke(null, new object[] { full }));
#endif
                if ((mode & 63) != 0) { throw new IOException("The staging filesystem does not preserve private Unix permissions."); }
            } catch {
                try { Directory.Delete(full, recursive: false); }
                catch (Exception cleanup) { TransferDiagnostics.RecordCleanupFailure("file", cleanup); }
                throw;
            }
            return;
        }
        string nativePath = WindowsSecurityPath(full);
        // Protected DACL, owner full control, inheritable by files and subdirectories.
        if (!ConvertPrivateDescriptor("D:P(A;OICI;FA;;;OW)", 1, out IntPtr descriptor, out _)) { throw NativeDirectoryError(); }
        try {
            PrivateSecurityAttributes attributes = new() { Length = Marshal.SizeOf<PrivateSecurityAttributes>(), Descriptor = descriptor };
            if (!CreateWindowsPrivateDirectory(nativePath, ref attributes)) { throw NativeDirectoryError(); }
        } finally { FreePrivateDescriptor(descriptor); }
        try {
            VerifyPrivateDirectoryProtection(nativePath);
        } catch {
            // No content has been placed in this exclusively created directory yet.
            try { Directory.Delete(full, recursive: false); }
            catch (Exception cleanup) { TransferDiagnostics.RecordCleanupFailure("file", cleanup); }
            throw;
        }
    }

    private static void VerifyPrivateDirectoryProtection(string path) {
        IntPtr buffer = ReadStagingSecurity(path);
        try {
            if (!GetPrivateDescriptorControl(buffer, out ushort control, out _)) { throw NativeDirectoryError(); }
            if ((control & 0x1004) != 0x1004) { // SE_DACL_PRESENT | SE_DACL_PROTECTED
                throw new IOException("The staging filesystem does not preserve protected access rules.");
            }
        } finally { Marshal.FreeHGlobal(buffer); }
    }

    private static void PreserveWindowsStagingPermissions(string staged, string destination) {
        IntPtr descriptor = ReadStagingSecurity(WindowsSecurityPath(Path.GetFullPath(destination)));
        try {
            if (!GetPrivateDescriptorControl(descriptor, out ushort control, out _)) { throw NativeDirectoryError(); }
            uint information = 4 | ((control & 0x1000) != 0 ? 0x80000000u : 0x20000000u);
            if (!SetStagingFileSecurity(WindowsSecurityPath(Path.GetFullPath(staged)), information, descriptor)) { throw NativeDirectoryError(); }
        } finally { Marshal.FreeHGlobal(descriptor); }
    }

    private static IntPtr ReadStagingSecurity(string path) {
        GetPrivateDirectorySecurity(path, 4, IntPtr.Zero, 0, out int length);
        if (length <= 0 || length > 65536) { throw NativeDirectoryError(); }
        IntPtr buffer = Marshal.AllocHGlobal(length);
        if (GetPrivateDirectorySecurity(path, 4, buffer, length, out _)) { return buffer; }
        int error = Marshal.GetLastWin32Error();
        Marshal.FreeHGlobal(buffer);
        throw new IOException("Could not read staging access rules.", new Win32Exception(error));
    }

    private static string WindowsSecurityPath(string full) => full.StartsWith(@"\\?\", StringComparison.Ordinal) ? full
        : full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full.Substring(2) : @"\\?\" + full;

    private static IOException NativeDirectoryError() => new("Could not establish staging permissions.", new Win32Exception(Marshal.GetLastWin32Error()));

    [StructLayout(LayoutKind.Sequential)]
    private struct PrivateSecurityAttributes {
        internal int Length;
        internal IntPtr Descriptor;
        [MarshalAs(UnmanagedType.Bool)] internal bool InheritHandle;
    }

    [DllImport("libc", EntryPoint = "mkdir", SetLastError = true)]
    private static extern int CreateUnixPrivateDirectory(byte[] path, uint mode);

    [DllImport("kernel32.dll", EntryPoint = "CreateDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateWindowsPrivateDirectory(string path, ref PrivateSecurityAttributes attributes);

    [DllImport("advapi32.dll", EntryPoint = "ConvertStringSecurityDescriptorToSecurityDescriptorW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ConvertPrivateDescriptor(string text, uint revision, out IntPtr descriptor, out uint size);

    [DllImport("advapi32.dll", EntryPoint = "GetFileSecurityW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPrivateDirectorySecurity(string path, uint requested, IntPtr buffer, int length, out int needed);

    [DllImport("advapi32.dll", EntryPoint = "SetFileSecurityW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetStagingFileSecurity(string path, uint information, IntPtr descriptor);

    [DllImport("advapi32.dll", EntryPoint = "GetSecurityDescriptorControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPrivateDescriptorControl(IntPtr descriptor, out ushort control, out uint revision);

    [DllImport("kernel32.dll", EntryPoint = "LocalFree")]
    private static extern IntPtr FreePrivateDescriptor(IntPtr descriptor);
}
