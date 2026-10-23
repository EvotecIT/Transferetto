using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Transferetto.Core;

public static partial class TransferFileSystem {
    /// <summary>Gets the case comparison of an existing destination directory, or its nearest existing ancestor.</summary>
    /// <remarks>This read-only query does not create probe files. Native Windows and macOS directory settings
    /// take precedence; other filesystems are checked using existing names, with the host default as fallback.</remarks>
    public static StringComparison GetPathComparison(string directory) {
        string current = Path.GetFullPath(directory);
        while (!Directory.Exists(current)) {
            string? parent = Path.GetDirectoryName(current);
            if (parent == null || parent == current) { break; }
            current = parent;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            using SafeFileHandle handle = OpenDirectory(current, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
            // FileCaseSensitiveInfo = 23; FILE_CS_FLAG_CASE_SENSITIVE_DIR = 1.
            if (!handle.IsInvalid && GetCaseSensitiveInfo(handle, 23, out uint flags, sizeof(uint))) {
                return (flags & 1) != 0 ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            }
            return StringComparison.OrdinalIgnoreCase;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            // Darwin _PC_CASE_SENSITIVE is 11. Query the actual volume, including case-sensitive APFS.
            long sensitive = MacPathConf(Encoding.UTF8.GetBytes(current + '\0'), 11);
            if (sensitive >= 0) { return sensitive == 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal; }
            return StringComparison.OrdinalIgnoreCase;
        }
        // Mounted filesystems on Unix can be case-insensitive. An alternative spelling that is absent
        // from enumeration but resolves successfully proves aliasing without modifying the destination.
        string[] entries = Directory.EnumerateFileSystemEntries(current).Take(32).ToArray();
        foreach (string entry in entries) {
            string name = Path.GetFileName(entry);
            string alternate = name.ToUpperInvariant();
            if (alternate == name) { alternate = name.ToLowerInvariant(); }
            if (alternate == name) { continue; }
            string candidate = Path.Combine(current, alternate);
            if (File.Exists(candidate) || Directory.Exists(candidate)) {
                // The alternative may be another real entry outside the bounded sample, not an alias.
                if (Directory.EnumerateFileSystemEntries(current).Any(other => Path.GetFileName(other) == alternate)) { continue; }
                return StringComparison.OrdinalIgnoreCase;
            }
            return StringComparison.Ordinal;
        }
        return StringComparison.Ordinal;
    }

    /// <summary>Gets a filesystem-wide lexical identity, independent of a transfer endpoint's selected root.</summary>
    /// <remarks>Uses each existing parent directory's comparison rules. On macOS, canonical Unicode spellings
    /// are also equivalent. Existing link checks remain the responsibility of the caller's rooted resolver.</remarks>
    public static string GetPathIdentity(string path) {
        string full = Path.GetFullPath(path);
        string volume = Path.GetPathRoot(full)!;
        string current = volume;
        string identity = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? volume.ToUpperInvariant() : volume;
        foreach (string segment in full.Substring(volume.Length).Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)) {
            string name = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? segment.Normalize(NormalizationForm.FormC) : segment;
            if (GetPathComparison(current) == StringComparison.OrdinalIgnoreCase) { name = name.ToUpperInvariant(); }
            identity = Path.Combine(identity, name);
            current = Path.Combine(current, segment);
        }
        return identity;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle OpenDirectory(string path, uint access, uint share, IntPtr security,
        uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCaseSensitiveInfo(SafeFileHandle handle, int informationClass, out uint flags, uint size);

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "pathconf", SetLastError = true)]
    private static extern long MacPathConf(byte[] path, int name);
}
