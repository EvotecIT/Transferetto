namespace Transferetto;
/// <summary>
/// Defines reusable SSH shell recipes for common remote administration flows.
/// </summary>

public enum TransferettoSshShellRecipeKind {
    /// <summary>
    /// Runs a command through <c>sudo</c> and supplies the password through the interactive shell when prompted.
    /// </summary>
    SudoCommand,
    /// <summary>
    /// Follows a remote file with <c>tail -f</c> until timeout, stop-pattern match, or cancellation.
    /// </summary>
    FollowFile,
    /// <summary>
    /// Follows systemd journal output for one service with <c>journalctl -f</c>.
    /// </summary>
    FollowJournal
}
