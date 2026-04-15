using System.Net;

namespace Transferetto;
/// <summary>
/// Represents connection settings for SSH-based sessions.
/// </summary>

public sealed class TransferettoSshConnectionOptions {
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
    /// Gets or sets the private Key Passphrase.
    /// </summary>

    public string? PrivateKeyPassphrase { get; set; }
    /// <summary>
    /// Gets or sets the remote port number.
    /// </summary>

    public int? Port { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether enable Keyboard Interactive.
    /// </summary>

    public bool EnableKeyboardInteractive { get; set; }
    /// <summary>
    /// Gets or sets the accept Any Host Key.
    /// </summary>

    public bool AcceptAnyHostKey { get; set; }
    /// <summary>
    /// Gets or sets the expected Host Key Fingerprints.
    /// </summary>

    public string[]? ExpectedHostKeyFingerprints { get; set; }
    /// <summary>
    /// Gets or sets the host Key Policy.
    /// </summary>

    public TransferettoSshHostKeyPolicy HostKeyPolicy { get; set; } = TransferettoSshHostKeyPolicy.TrustOnFirstUse;
    /// <summary>
    /// Gets or sets the known Hosts Path.
    /// </summary>

    public string? KnownHostsPath { get; set; }
    /// <summary>
    /// Gets or sets the keep Alive Interval Seconds.
    /// </summary>

    public int? KeepAliveIntervalSeconds { get; set; }
    /// <summary>
    /// Gets or sets the connection Timeout Seconds.
    /// </summary>

    public int? ConnectionTimeoutSeconds { get; set; }
    /// <summary>
    /// Gets or sets the retry Attempts.
    /// </summary>

    public int? RetryAttempts { get; set; }
    /// <summary>
    /// Gets or sets the proxy Type.
    /// </summary>

    public TransferettoSshProxyType ProxyType { get; set; }
    /// <summary>
    /// Gets or sets the proxy Host.
    /// </summary>

    public string? ProxyHost { get; set; }
    /// <summary>
    /// Gets or sets the proxy Port.
    /// </summary>

    public int? ProxyPort { get; set; }
    /// <summary>
    /// Gets or sets the proxy Credential.
    /// </summary>

    public NetworkCredential? ProxyCredential { get; set; }
}
