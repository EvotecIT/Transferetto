using System;

namespace Transferetto;

internal interface ISftpTransferDirectoryOperations {
    bool IsDirectory(string path);

    void CreateDirectory(string path);
}

internal sealed class SftpTransferDirectoryOperations : ISftpTransferDirectoryOperations {
    private readonly TransferettoSftpSession _session;

    internal SftpTransferDirectoryOperations(TransferettoSftpSession session) {
        _session = session;
    }

    public bool IsDirectory(string path) => TransferettoClient.TestSftpDirectory(_session, path);

    public void CreateDirectory(string path) => _session.Client.CreateDirectory(path);
}

internal static class SftpTransferDirectory {
    internal static void Ensure(ISftpTransferDirectoryOperations operations, string path) {
        bool absolute = path.StartsWith("/", StringComparison.Ordinal);
        string current = absolute ? "/" : string.Empty;
        foreach (string segment in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)) {
            current = current == "/"
                ? current + segment
                : string.IsNullOrEmpty(current) ? segment : current + "/" + segment;
            if (operations.IsDirectory(current)) {
                continue;
            }

            try {
                operations.CreateDirectory(current);
            } catch {
                if (!operations.IsDirectory(current)) {
                    throw;
                }
            }
        }
    }
}
