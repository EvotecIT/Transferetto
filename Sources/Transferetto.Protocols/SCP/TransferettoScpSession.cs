using System;
using Renci.SshNet;

namespace Transferetto;
/// <summary>
/// Represents a reusable SCP session.
/// </summary>

public sealed class TransferettoScpSession : IDisposable {
    internal TransferettoScpSession(ScpClient client, string? error = null) {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Error = error;
    }

    internal ScpClient Client { get; }
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
    /// Gets or sets the host Key Info.
    /// </summary>

    public TransferettoSshHostKeyInfo? HostKeyInfo { get; internal set; }
    /// <summary>
    /// Releases resources held by the SCP session.
    /// </summary>

    public void Dispose() {
        if (Client.IsConnected) {
            Client.Disconnect();
        }

        Client.Dispose();
    }
}
