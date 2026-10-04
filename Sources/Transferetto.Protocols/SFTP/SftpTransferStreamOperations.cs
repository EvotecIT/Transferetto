using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Transferetto;

/// <summary>Owns SFTP stream creation and replacement permissions before content is written.</summary>
internal static class SftpTransferStreamOperations {
    internal static string AnchorCleanupPath(SftpClient client, string path) {
        if (path.StartsWith("/", StringComparison.Ordinal)) { return path; }
        string workingDirectory = client.WorkingDirectory;
        return workingDirectory.TrimEnd('/') + "/" + path;
    }

    internal static async Task RemoveTemporaryFileAsync(TransferettoSftpSession session, string path) {
        using CancellationTokenSource cleanup = new(TimeSpan.FromSeconds(10));
        try {
            if (session.Client.IsConnected) {
                await session.Client.DeleteFileAsync(path, cleanup.Token).ConfigureAwait(false);
                return;
            }
            string? accepted = session.HostKeyInfo?.FingerPrintSHA256;
            if (string.IsNullOrEmpty(accepted)) { return; }
            // Cancellation during acquisition closes the original connection. Use a separately
            // pinned connection for cleanup rather than restoring or silently reusing that session.
            using SftpClient client = new(session.Client.ConnectionInfo);
            client.HostKeyReceived += (_, args) => args.CanTrust = string.Equals(accepted, args.FingerPrintSHA256, StringComparison.Ordinal);
            await client.ConnectAsync(cleanup.Token).ConfigureAwait(false);
            await client.DeleteFileAsync(path, cleanup.Token).ConfigureAwait(false);
        } catch {
            // Cleanup must not replace the transfer failure when the server is unavailable.
        }
    }

    internal static async Task<Stream> OpenAsync(SftpClient client, string path, FileMode mode, FileAccess access,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        Stream? opened = null;
        // A canceled SDK open can discard a late HANDLE response. Tear down the session during
        // acquisition instead, so the server releases every handle belonging to that connection.
        using CancellationTokenRegistration registration = cancellationToken.Register(() => Disconnect(client));
        try {
            opened = await client.OpenAsync(path, mode, access, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            Stream result = opened;
            opened = null;
            return result;
        } catch (Exception) when (cancellationToken.IsCancellationRequested) {
            Disconnect(client);
            throw new OperationCanceledException(cancellationToken);
        } catch {
            Disconnect(client);
            throw;
        } finally {
            opened?.Dispose();
        }
    }

    internal static async Task<SftpFileAttributes?> GetExistingAttributesAsync(SftpClient client, string path,
        CancellationToken cancellationToken) {
        try { return await client.GetAttributesAsync(path, cancellationToken).ConfigureAwait(false); }
        catch (SftpPathNotFoundException) { return null; }
    }

    internal static async Task PreserveReplacementAttributesAsync(SftpClient client, string stagedPath,
        SftpFileAttributes? existing, CancellationToken cancellationToken) {
        if (existing == null) { return; }
        // Apply restrictive access before writing any content, then preserve owner and group.
        short permissions = short.Parse(TransferettoSftpAttributes.FromFileAttributes(stagedPath, existing).PermissionsOctal,
            System.Globalization.CultureInfo.InvariantCulture);
        client.ChangePermissions(stagedPath, permissions);
        SftpFileAttributes staged = await client.GetAttributesAsync(stagedPath, cancellationToken).ConfigureAwait(false);
        staged.UserId = existing.UserId;
        staged.GroupId = existing.GroupId;
        client.SetAttributes(stagedPath, staged);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void Disconnect(SftpClient client) {
        try { client.Disconnect(); }
        catch { /* Preserve the transfer failure; disconnect is best effort on a broken transport. */ }
    }
}
