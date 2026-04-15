using System;
using FluentFTP;

namespace Transferetto;
/// <summary>
/// Represents a reusable FTP or FTPS session.
/// </summary>

public sealed class TransferettoFtpSession : IDisposable {
    internal TransferettoFtpSession(FtpClient client, FtpProfile? autoDetectedProfile = null, string? error = null) {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        AutoDetectedProfile = autoDetectedProfile;
        Error = error;
    }

    internal FtpClient Client { get; }
    /// <summary>
    /// Gets the remote host name.
    /// </summary>

    public string Host => Client.Host;
    /// <summary>
    /// Gets the remote port number.
    /// </summary>

    public int Port => Client.Port;
    /// <summary>
    /// Gets a value indicating whether connected.
    /// </summary>

    public bool IsConnected => Client.IsConnected;
    /// <summary>
    /// Gets or sets the error message associated with the current session or operation.
    /// </summary>

    public string? Error { get; internal set; }
    /// <summary>
    /// Gets the auto Detected Profile.
    /// </summary>

    public FtpProfile? AutoDetectedProfile { get; }
    /// <summary>
    /// Releases resources held by the FTP or FTPS session.
    /// </summary>

    public void Dispose() {
        if (Client.IsConnected) {
            Client.Disconnect();
        }

        Client.Dispose();
    }
}
