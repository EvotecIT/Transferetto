using System;

namespace Transferetto;

internal sealed class FtpTransferCommitOperations : IProtocolTransferCommitOperations {
    private readonly TransferettoFtpSession _session;

    internal FtpTransferCommitOperations(TransferettoFtpSession session) {
        _session = session;
    }

    public bool Exists(string path) => TransferettoClient.TestFtpFile(_session, path);

    public void Delete(string path) => TransferettoClient.RemoveFtpFile(_session, path);

    public void Rename(string sourcePath, string destinationPath, bool overwriteAtomically) {
        if (overwriteAtomically) {
            throw new NotSupportedException("FTP does not provide a portable atomic overwrite rename.");
        }
        TransferettoClient.RenameFtpFile(_session, sourcePath, destinationPath);
    }
}
