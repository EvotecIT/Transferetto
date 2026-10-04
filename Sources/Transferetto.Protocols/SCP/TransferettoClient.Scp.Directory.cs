using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
    private static async Task ReceiveScpDirectoryAsync(TransferettoScpSession session, string remotePath, string localPath,
        Action<string, long, long> progress, CancellationToken cancellationToken) {
        if (!session.IsConnected) { throw new InvalidOperationException("The SCP session is not connected."); }
        string? acceptedFingerprint = session.HostKeyInfo?.FingerPrintSHA256;
        if (string.IsNullOrEmpty(acceptedFingerprint)) { throw new InvalidOperationException("The SCP session has no accepted host key."); }
        // SCP's directory records are received over a separate command connection pinned to
        // the already-accepted host identity. The caller's existing SCP session stays reusable.
        using SshClient transport = new(session.Client.ConnectionInfo);
        transport.HostKeyReceived += (_, args) => args.CanTrust = string.Equals(
            NormalizeFingerprint(acceptedFingerprint), NormalizeFingerprint(args.FingerPrintSHA256), StringComparison.Ordinal);
        await transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
        using SshCommand command = transport.CreateCommand("scp -prf " + RemotePathTransformation.ShellQuote.Transform(remotePath));
        command.CommandTimeout = session.Client.OperationTimeout;
        Task execution = command.ExecuteAsync(cancellationToken);
        Task stderr = TransferContent.CopyToAsync(command.ExtendedOutputStream, Stream.Null, null, cancellationToken);
        try {
            using (Stream input = command.CreateInputStream()) {
                ScpDirectoryReceiver receiver = new(command.OutputStream, input, localPath, progress);
                await receiver.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            }
            await execution.ConfigureAwait(false);
            await stderr.ConfigureAwait(false);
            if (command.ExitStatus != 0) { throw new IOException("The SCP server did not complete the directory transfer successfully."); }
        } catch {
            TryCancelSshCommand(command);
            transport.Disconnect();
            try { await execution.ConfigureAwait(false); } catch { }
            try { await stderr.ConfigureAwait(false); } catch { }
            throw;
        }
    }
}
