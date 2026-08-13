namespace Transferetto;
/// <summary>
/// Stores process-wide runtime settings used by Transferetto.
/// </summary>

public static class TransferettoRuntimeSettings {
    /// <summary>
    /// Gets or sets the FTP tracing options applied to new FTP sessions.
    /// </summary>
    public static TransferettoFtpTraceOptions? FtpTraceOptions { get; set; }
}
