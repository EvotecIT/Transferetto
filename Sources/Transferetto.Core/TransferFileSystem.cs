using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#if !NET8_0_OR_GREATER
using System.Reflection;
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace Transferetto.Core;

/// <summary>Provides rooted local path validation, safe traversal, and same-directory atomic file commits.</summary>
/// <remarks>The caller must control the root and its ancestors. Checks reject links at and beneath the selected
/// root, but do not provide an OS sandbox against concurrent directory mutation by another process.</remarks>
public static class TransferFileSystem {
#if !NET8_0_OR_GREATER
    private static readonly MethodInfo? GetUnixMode = typeof(File).GetMethod("GetUnixFileMode", new[] { typeof(string) });
    private static readonly MethodInfo? SetUnixMode = GetUnixMode == null ? null
        : typeof(File).GetMethod("SetUnixFileMode", new[] { typeof(string), GetUnixMode.ReturnType });
#endif
    /// <summary>Preserves the destination's Unix permission bits on a staged replacement before content is written.</summary>
    public static void PreserveStagingPermissions(string stagedPath, string destinationPath) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !File.Exists(destinationPath)) { return; }
#if NET8_0_OR_GREATER
        File.SetUnixFileMode(stagedPath, File.GetUnixFileMode(destinationPath));
#else
        // The netstandard assembly also runs on modern Unix runtimes. Resolve their file-mode
        // APIs once without taking a dependency on a runtime-specific enum in the public contract.
        if (GetUnixMode == null || SetUnixMode == null) {
            throw new PlatformNotSupportedException("Safe file replacement on Unix requires a runtime with Unix file-mode APIs.");
        }
        try {
            SetUnixMode.Invoke(null, new[] { (object)stagedPath, GetUnixMode.Invoke(null, new object[] { destinationPath })! });
        } catch (TargetInvocationException exception) when (exception.InnerException != null) {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
#endif
    }

    private static StringComparison PathComparison => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    /// <summary>Maps a slash-separated relative name to a local path, rejecting traversal and unrepresentable names.</summary>
    public static string ResolveRelativePath(string root, string relativePath, bool allowEmpty = false) {
        if (root == null) { throw new ArgumentNullException(nameof(root)); }
        if (relativePath == null) { throw new ArgumentNullException(nameof(relativePath)); }
        if (relativePath.Length == 0 && !allowEmpty) { throw new ArgumentException("A relative path is required.", nameof(relativePath)); }
        if (Path.IsPathRooted(relativePath)) { throw new ArgumentException("The local name must be relative to its root.", nameof(relativePath)); }
        bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        foreach (string segment in relativePath.Split('/')) {
            if (segment == "." || segment == ".." || (segment.Length == 0 && relativePath.Length != 0)) {
                throw new ArgumentException("Empty and relative directory segments are not allowed.", nameof(relativePath));
            }
            if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || (windows && IsWindowsAlias(segment))) {
                throw new ArgumentException($"The remote name cannot be represented safely on this filesystem: {segment}", nameof(relativePath));
            }
        }
        string candidate = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureNoLinkTraversal(root, candidate);
        return candidate;
    }

    /// <summary>Rejects paths outside the root and symbolic links or reparse points at or beneath it.</summary>
    public static void EnsureNoLinkTraversal(string root, string candidate) {
        string fullRoot = NormalizeRoot(root);
        string fullCandidate = Path.GetFullPath(candidate);
        string prefix = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullCandidate.Equals(fullRoot, PathComparison) && !fullCandidate.StartsWith(prefix, PathComparison)) {
            throw new ArgumentException("The path escapes the selected filesystem root.", nameof(candidate));
        }
        RejectLink(fullRoot);
        string current = fullRoot;
        string relative = fullCandidate.Equals(fullRoot, PathComparison) ? string.Empty : fullCandidate.Substring(prefix.Length);
        foreach (string segment in relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)) {
            current = Path.Combine(current, segment);
            RejectLink(current);
        }
    }

    /// <summary>Enumerates local entries without following links, with cancellation before each entry and descent.</summary>
    public static IEnumerable<FileSystemInfo> EnumerateEntries(string root, bool recursive = true, CancellationToken cancellationToken = default) {
        string fullRoot = NormalizeRoot(root);
        Stack<string> pending = new();
        pending.Push(fullRoot);
        while (pending.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();
            string directory = pending.Pop();
            EnsureNoLinkTraversal(fullRoot, directory);
            foreach (FileSystemInfo entry in new DirectoryInfo(directory).EnumerateFileSystemInfos()) {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoLinkTraversal(fullRoot, entry.FullName);
                yield return entry;
                if (recursive && entry is DirectoryInfo) { pending.Push(entry.FullName); }
            }
        }
    }

    /// <summary>Commits a staged file without deleting the old destination first. Both paths must share a directory.</summary>
    public static void CommitStagedFile(string stagedPath, string destinationPath, bool overwrite = true) {
        string staged = Path.GetFullPath(stagedPath);
        string destination = Path.GetFullPath(destinationPath);
        string directory = Path.GetDirectoryName(destination)!;
        if (!string.Equals(Path.GetDirectoryName(staged), directory, PathComparison)) {
            throw new ArgumentException("Atomic file commits require staging in the destination directory.", nameof(stagedPath));
        }
        EnsureNoLinkTraversal(directory, staged);
        EnsureNoLinkTraversal(directory, destination);
        if (!overwrite) { File.Move(staged, destination); return; }
        if (!File.Exists(destination)) {
            try { File.Move(staged, destination); return; }
            catch (IOException) when (File.Exists(destination)) { }
        }
        EnsureNoLinkTraversal(directory, destination);
        File.Replace(staged, destination, null);
    }

    private static void RejectLink(string path) {
        FileAttributes attributes;
        try { attributes = File.GetAttributes(path); }
        catch (FileNotFoundException) { return; }
        catch (DirectoryNotFoundException) { return; }
        if ((attributes & FileAttributes.ReparsePoint) != 0) {
            throw new IOException($"Symbolic links and reparse points are not allowed at or beneath the selected root: {path}");
        }
    }

    private static string NormalizeRoot(string root) {
        string fullPath = Path.GetFullPath(root);
        string volume = Path.GetPathRoot(fullPath)!;
        string trimmed = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return trimmed.Length < volume.Length ? volume : trimmed;
    }

    private static bool IsWindowsAlias(string segment) {
        if (segment.EndsWith(".", StringComparison.Ordinal) || segment.EndsWith(" ", StringComparison.Ordinal)) { return true; }
        string name = segment.Split('.')[0];
        if (name.Equals("CON", StringComparison.OrdinalIgnoreCase) || name.Equals("PRN", StringComparison.OrdinalIgnoreCase)
            || name.Equals("AUX", StringComparison.OrdinalIgnoreCase) || name.Equals("NUL", StringComparison.OrdinalIgnoreCase)) { return true; }
        return name.Length == 4 && (name.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || name.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
            && "123456789¹²³".IndexOf(name[3]) >= 0;
    }
}
