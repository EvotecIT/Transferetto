using System.Net;

namespace Transferetto;
/// <summary>
/// Represents connection settings for an SFTP session.
/// </summary>

public sealed class TransferettoSftpConnectionOptions {
    /// <summary>
    /// Gets or sets the server.
    /// </summary>
    public string Server { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the user Name.
    /// </summary>

    public string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the password.
    /// </summary>

    public string? Password { get; set; }
    /// <summary>
    /// Gets or sets the credential.
    /// </summary>

    public NetworkCredential? Credential { get; set; }
    /// <summary>
    /// Gets or sets the private Key Path.
    /// </summary>

    public string? PrivateKeyPath { get; set; }
    /// <summary>
    /// Gets or sets the remote port number.
    /// </summary>

    public int? Port { get; set; }
}
