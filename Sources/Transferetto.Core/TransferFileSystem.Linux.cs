using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Transferetto.Core;

public static partial class TransferFileSystem {
    // Linux statfs starts with one native word containing f_type. The oversized output buffer avoids
    // depending on the remainder of the architecture-specific statfs layout (we never inspect it).
    private static StringComparison? GetLinuxPathComparison(string directory) {
        byte[] path = Encoding.UTF8.GetBytes(directory + '\0');
        byte[] information = new byte[256];
        if (LinuxStatFs(path, information) != 0) { return null; }
        uint type = unchecked((uint)(IntPtr.Size == 8
            ? BitConverter.ToInt64(information, 0) : BitConverter.ToInt32(information, 0)));
        if (type == 0xEF53 || type == 0xF2F52010) { // ext4 and f2fs support per-directory casefold.
            int handle = LinuxOpen(path, 0x90000); // O_RDONLY | O_DIRECTORY | O_CLOEXEC
            if (handle < 0) { return null; }
            try {
                UIntPtr request = new(IntPtr.Size == 8 ? 0x80086601UL : 0x80046601UL); // FS_IOC_GETFLAGS
                if (LinuxGetFlags(handle, request, out long flags) != 0) { return null; }
                return (flags & 0x40000000) != 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            } finally { LinuxClose(handle); }
        }
        // Filesystems with case-sensitive directory lookup. Network/FUSE/unknown mounts are not inferred
        // from the host; existing-name evidence or the conservative fallback handles those separately.
        return type == 0x01021994 || type == 0x58465342 || type == 0x9123683E || type == 0x794C7630
            ? StringComparison.Ordinal : null; // tmpfs, XFS, btrfs, overlayfs
    }

    [DllImport("libc", EntryPoint = "statfs", SetLastError = true)]
    private static extern int LinuxStatFs(byte[] path, [Out] byte[] information);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int LinuxOpen(byte[] path, int flags);

    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    private static extern int LinuxGetFlags(int handle, UIntPtr request, out long flags);

    [DllImport("libc", EntryPoint = "close")]
    private static extern int LinuxClose(int handle);
}
