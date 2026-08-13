namespace Transferetto;

internal sealed class SftpTransferCommitOperations : IProtocolTransferCommitOperations {
    private readonly TransferettoSftpSession _session;

    internal SftpTransferCommitOperations(TransferettoSftpSession session) {
        _session = session;
    }

    public bool SupportsNoClobberRename => true;

    public bool Exists(string path) => _session.Client.Exists(path);

    public void Delete(string path) => TransferettoClient.RemoveSftpFile(_session, path);

    public void Rename(string sourcePath, string destinationPath, bool overwriteAtomically) =>
        TransferettoClient.MoveSftpFile(_session, sourcePath, destinationPath, overwriteAtomically);
}
