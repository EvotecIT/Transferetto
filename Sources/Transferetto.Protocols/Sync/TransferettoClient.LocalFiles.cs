using System.IO;
using System.Threading;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
    private static void EnsureSafeLocalFilePath(string path) {
        string fullPath = Path.GetFullPath(path);
        TransferFileSystem.EnsureNoLinkTraversal(Path.GetDirectoryName(fullPath)!, fullPath);
    }

    private static void EnsureSafeLocalDirectoryTree(string path, CancellationToken cancellationToken = default) {
        TransferFileSystem.EnsureNoLinkTraversal(path, path);
        if (Directory.Exists(path)) {
            foreach (FileSystemInfo entry in TransferFileSystem.EnumerateEntries(path, cancellationToken: cancellationToken)) {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
