using System;
using Renci.SshNet;

namespace Transferetto;
/// <summary>
/// Represents a reusable SSH tunnel session.
/// </summary>

public sealed class TransferettoSshTunnelSession : IDisposable {
    private bool _removed;

    internal TransferettoSshTunnelSession(
        TransferettoSshSession sshSession,
        ForwardedPort forwardedPort,
        TransferettoSshTunnelType tunnelType,
        string boundHost,
        uint boundPort,
        string host,
        uint port) {
        SshSession = sshSession ?? throw new ArgumentNullException(nameof(sshSession));
        ForwardedPort = forwardedPort ?? throw new ArgumentNullException(nameof(forwardedPort));
        TunnelType = tunnelType;
        BoundHost = boundHost;
        BoundPort = boundPort;
        Host = host;
        Port = port;
    }

    internal TransferettoSshSession SshSession { get; }

    internal ForwardedPort ForwardedPort { get; }
    /// <summary>
    /// Gets the tunnel Type.
    /// </summary>

    public TransferettoSshTunnelType TunnelType { get; }
    /// <summary>
    /// Gets the remote host name.
    /// </summary>

    public string Host { get; }
    /// <summary>
    /// Gets the remote port number.
    /// </summary>

    public uint Port { get; }
    /// <summary>
    /// Gets the bound Host.
    /// </summary>

    public string BoundHost { get; }
    /// <summary>
    /// Gets the bound Port.
    /// </summary>

    public uint BoundPort { get; }
    /// <summary>
    /// Gets a value indicating whether started.
    /// </summary>

    public bool IsStarted => ForwardedPort.IsStarted;
    /// <summary>
    /// Gets a value indicating whether connected.
    /// </summary>

    public bool IsConnected => SshSession.IsConnected;
    /// <summary>
    /// Releases resources held by the SSH session.
    /// </summary>

    public void Dispose() {
        Stop();
        ForwardedPort.Dispose();
    }

    internal void Stop() {
        if (ForwardedPort.IsStarted) {
            ForwardedPort.Stop();
        }

        if (!_removed && SshSession.IsConnected) {
            SshSession.Client.RemoveForwardedPort(ForwardedPort);
            _removed = true;
        }
    }
}
