namespace Transferetto;
/// <summary>
/// Defines reusable SSH shell prompt presets for common interactive environments.
/// </summary>

public enum TransferettoSshShellPromptPreset {
    /// <summary>
    /// No preset prompt pattern is applied.
    /// </summary>
    None,
    /// <summary>
    /// Matches a typical Linux or POSIX shell prompt ending with <c>$</c> or <c>#</c>.
    /// </summary>
    Linux,
    /// <summary>
    /// Matches a typical non-root Linux or POSIX shell prompt ending with <c>$</c>.
    /// </summary>
    LinuxUser,
    /// <summary>
    /// Matches a typical root Linux or POSIX shell prompt ending with <c>#</c>.
    /// </summary>
    LinuxRoot,
    /// <summary>
    /// Matches a typical PowerShell prompt beginning with <c>PS</c> and ending with <c>&gt;</c>.
    /// </summary>
    PowerShell,
    /// <summary>
    /// Matches a typical Windows Command Prompt ending with <c>&gt;</c>.
    /// </summary>
    Cmd
}
