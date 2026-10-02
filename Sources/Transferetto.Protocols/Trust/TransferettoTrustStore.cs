using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Transferetto.Core;

namespace Transferetto;

/// <summary>Serializes trust decisions across cooperating processes and commits complete stores atomically.</summary>
internal static class TransferettoTrustStore {
    internal static FileStream Acquire(string path) {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath)!;
        TransferFileSystem.EnsureNoLinkTraversal(directory, fullPath);
        Directory.CreateDirectory(directory);
        string lockPath = fullPath + ".lock";
        TransferFileSystem.EnsureNoLinkTraversal(directory, lockPath);
        Stopwatch timeout = Stopwatch.StartNew();
        while (true) {
            try { return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) when (timeout.Elapsed < TimeSpan.FromSeconds(10)) { Thread.Sleep(25); }
        }
    }

    internal static void Save(string path, string[] lines) {
        string fullPath = Path.GetFullPath(path);
        string staged = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (FileStream stream = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                using (StreamWriter writer = new(stream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: true)) {
                    foreach (string line in lines) { writer.WriteLine(line); }
                }
                stream.Flush(flushToDisk: true);
            }
            TransferFileSystem.CommitStagedFile(staged, fullPath);
        } finally {
            if (File.Exists(staged)) { File.Delete(staged); }
        }
    }

    internal static string[] Read(string path) {
        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 0) { throw new InvalidDataException("An existing trust store is empty. Restore it or remove it explicitly to reset trust."); }
        return lines;
    }

    internal static void ValidateRecord(string[] parts, string path) {
        if (parts.Length != 8 || string.IsNullOrWhiteSpace(parts[0])
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int port) || port < 1 || port > 65535
            || !DateTimeOffset.TryParseExact(parts[6], "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            || !DateTimeOffset.TryParseExact(parts[7], "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) {
            throw new InvalidDataException($"The trust store contains an invalid record: {path}");
        }
    }

    internal static string SerializeValue(string? value) {
        string result = value ?? string.Empty;
        if (result.IndexOfAny(new[] { '\t', '\r', '\n' }) >= 0) {
            throw new InvalidDataException("Trust-store fields cannot contain tabs or line breaks.");
        }
        return result;
    }

    internal static bool IsHexFingerprint(string value, int length) => value.Length == length
        && value.All(character => character >= '0' && character <= '9' || character >= 'a' && character <= 'f' || character >= 'A' && character <= 'F');
}
