namespace Transferetto;
/// <summary>
/// Represents terminal and transcript settings for an SSH shell session.
/// </summary>

public sealed class TransferettoSshShellOptions {
    private const int DefaultMaxTranscriptEntries = 500;
    private const int DefaultMaxTranscriptCharacters = 262144;
    /// <summary>
    /// Gets or sets the terminal Name.
    /// </summary>

    public string TerminalName { get; set; } = "xterm-256color";
    /// <summary>
    /// Gets or sets the columns.
    /// </summary>

    public uint Columns { get; set; } = 120;
    /// <summary>
    /// Gets or sets the rows.
    /// </summary>

    public uint Rows { get; set; } = 40;
    /// <summary>
    /// Gets or sets the width.
    /// </summary>

    public uint Width { get; set; }
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    public uint Height { get; set; }
    /// <summary>
    /// Gets or sets the buffer Size.
    /// </summary>

    public int BufferSize { get; set; } = 4096;
    /// <summary>
    /// Gets or sets the no Terminal.
    /// </summary>

    public bool NoTerminal { get; set; }
    /// <summary>
    /// Gets or sets the prompt Pattern.
    /// </summary>

    public string? PromptPattern { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether enable Transcript.
    /// </summary>

    public bool EnableTranscript { get; set; } = true;
    /// <summary>
    /// Gets or sets the max Transcript Entries.
    /// </summary>

    public int MaxTranscriptEntries { get; set; } = DefaultMaxTranscriptEntries;
    /// <summary>
    /// Gets or sets the max Transcript Characters.
    /// </summary>

    public int MaxTranscriptCharacters { get; set; } = DefaultMaxTranscriptCharacters;
}
