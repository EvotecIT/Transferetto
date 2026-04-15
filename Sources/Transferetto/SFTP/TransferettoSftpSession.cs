using System;
using Renci.SshNet;

namespace Transferetto;
/// <summary>
/// Represents a reusable SFTP session.
/// </summary>

public sealed class TransferettoSftpSession : IDisposable {
    internal TransferettoSftpSession(SftpClient client, string? error = null) {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Error = error;
    }

    internal SftpClient Client { get; }
    /// <summary>
    /// Gets the remote host name.
    /// </summary>

    public string Host => Client.ConnectionInfo.Host;
    /// <summary>
    /// Gets the remote port number.
    /// </summary>

    public int Port => Client.ConnectionInfo.Port;
    /// <summary>
    /// Gets a value indicating whether connected.
    /// </summary>

    public bool IsConnected => Client.IsConnected;
    /// <summary>
    /// Gets or sets the error message associated with the current session or operation.
    /// </summary>

    public string? Error { get; internal set; }
    /// <summary>
    /// Create Directory.
    /// </summary>

    public void CreateDirectory(string path) {
        Client.CreateDirectory(path);
    }
    /// <summary>
    /// Releases resources held by the SFTP session.
    /// </summary>

    public void Dispose() {
        if (Client.IsConnected) {
            Client.Disconnect();
        }

        Client.Dispose();
    }
}
