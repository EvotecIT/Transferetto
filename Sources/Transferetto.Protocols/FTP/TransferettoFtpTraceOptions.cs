namespace Transferetto;
/// <summary>
/// Controls FTP tracing behavior for Transferetto sessions.
/// </summary>

public sealed class TransferettoFtpTraceOptions {
    /// <summary>
    /// Gets or sets a value indicating whether log To Console.
    /// </summary>
    public bool LogToConsole { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether log User Name.
    /// </summary>

    public bool LogUserName { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether log Password.
    /// </summary>

    public bool LogPassword { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether log Host.
    /// </summary>

    public bool LogHost { get; set; }
}
