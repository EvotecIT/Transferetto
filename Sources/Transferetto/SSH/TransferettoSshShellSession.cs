using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;

namespace Transferetto;
/// <summary>
/// Represents a reusable interactive SSH shell session.
/// </summary>

public sealed class TransferettoSshShellSession : IDisposable {
    private readonly object _pendingReadSync = new();
    private readonly object _transcriptSync = new();
    private readonly List<TransferettoSshShellTranscriptEntry> _transcriptEntries = new();
    private string _pendingReadOutput = string.Empty;
    private int _transcriptCharacterCount;
    private int _droppedTranscriptEntryCount;

    internal TransferettoSshShellSession(TransferettoSshSession sshSession, ShellStream shellStream, TransferettoSshShellOptions options) {
        SshSession = sshSession ?? throw new ArgumentNullException(nameof(sshSession));
        ShellStream = shellStream ?? throw new ArgumentNullException(nameof(shellStream));
        TerminalName = options.TerminalName;
        Columns = options.Columns;
        Rows = options.Rows;
        Width = options.Width;
        Height = options.Height;
        BufferSize = options.BufferSize;
        NoTerminal = options.NoTerminal;
        PromptPreset = options.PromptPreset;
        PromptPattern = TransferettoClient.ResolveSshShellPromptPattern(options.PromptPattern, options.PromptPreset);
        EnableTranscript = options.EnableTranscript;
        MaxTranscriptEntries = options.MaxTranscriptEntries > 0 ? options.MaxTranscriptEntries : 500;
        MaxTranscriptCharacters = options.MaxTranscriptCharacters > 0 ? options.MaxTranscriptCharacters : 262144;
    }

    internal TransferettoSshSession SshSession { get; }

    internal ShellStream ShellStream { get; }
    /// <summary>
    /// Gets the remote host name.
    /// </summary>

    public string Host => SshSession.Host;
    /// <summary>
    /// Gets the remote port number.
    /// </summary>

    public int Port => SshSession.Port;
    /// <summary>
    /// Gets a value indicating whether connected.
    /// </summary>

    public bool IsConnected => SshSession.IsConnected;
    /// <summary>
    /// Gets the data Available.
    /// </summary>

    public bool DataAvailable => HasPendingReadOutput() || ShellStream.DataAvailable;
    /// <summary>
    /// Gets the terminal Name.
    /// </summary>

    public string TerminalName { get; }
    /// <summary>
    /// Gets or sets the columns.
    /// </summary>

    public uint Columns { get; private set; }
    /// <summary>
    /// Gets or sets the rows.
    /// </summary>

    public uint Rows { get; private set; }
    /// <summary>
    /// Gets or sets the width.
    /// </summary>

    public uint Width { get; private set; }
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    public uint Height { get; private set; }
    /// <summary>
    /// Gets the buffer Size.
    /// </summary>

    public int BufferSize { get; }
    /// <summary>
    /// Gets the no Terminal.
    /// </summary>

    public bool NoTerminal { get; }
    /// <summary>
    /// Gets or sets the prompt Pattern.
    /// </summary>

    public string? PromptPattern { get; private set; }
    /// <summary>
    /// Gets the configured prompt preset.
    /// </summary>

    public TransferettoSshShellPromptPreset PromptPreset { get; private set; }
    /// <summary>
    /// Gets a value indicating whether enable Transcript.
    /// </summary>

    public bool EnableTranscript { get; }
    /// <summary>
    /// Gets the max Transcript Entries.
    /// </summary>

    public int MaxTranscriptEntries { get; }
    /// <summary>
    /// Gets the max Transcript Characters.
    /// </summary>

    public int MaxTranscriptCharacters { get; }

    internal void UpdateWindowSize(uint columns, uint rows, uint width, uint height) {
        Columns = columns;
        Rows = rows;
        Width = width;
        Height = height;
    }

    internal void UpdatePromptPattern(string? promptPattern, TransferettoSshShellPromptPreset promptPreset) {
        PromptPattern = promptPattern;
        PromptPreset = promptPreset;
    }

    internal bool HasPendingReadOutput() {
        lock (_pendingReadSync) {
            return !string.IsNullOrEmpty(_pendingReadOutput);
        }
    }

    internal string ConsumePendingReadOutput() {
        lock (_pendingReadSync) {
            string pendingReadOutput = _pendingReadOutput;
            _pendingReadOutput = string.Empty;
            return pendingReadOutput;
        }
    }

    internal void AppendPendingReadOutput(string? text) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }

        lock (_pendingReadSync) {
            _pendingReadOutput += text;
        }
    }

    internal void RecordTranscript(TransferettoSshShellTranscriptDirection direction, string? text) {
        if (!EnableTranscript || string.IsNullOrEmpty(text)) {
            return;
        }

        string normalizedText = NormalizeTranscriptText(text!, MaxTranscriptCharacters);
        TransferettoSshShellTranscriptEntry entry = new() {
            TimestampUtc = DateTime.UtcNow,
            Direction = direction,
            Text = normalizedText
        };

        lock (_transcriptSync) {
            _transcriptEntries.Add(entry);
            _transcriptCharacterCount += normalizedText.Length;
            TrimTranscript();
        }
    }

    internal TransferettoSshShellTranscriptSnapshot GetTranscript(int? lastEntries = null) {
        lock (_transcriptSync) {
            IReadOnlyList<TransferettoSshShellTranscriptEntry> entries = _transcriptEntries.ToArray();
            int droppedEntryCount = _droppedTranscriptEntryCount;

            if (lastEntries.HasValue && lastEntries.Value > 0 && entries.Count > lastEntries.Value) {
                int omitted = entries.Count - lastEntries.Value;
                entries = entries.Skip(omitted).ToArray();
                droppedEntryCount += omitted;
            }

            return new TransferettoSshShellTranscriptSnapshot {
                Entries = entries,
                CapturedEntryCount = entries.Count,
                DroppedEntryCount = droppedEntryCount,
                TotalCharacterCount = entries.Sum(static entry => entry.Text.Length)
            };
        }
    }

    internal TransferettoSshShellTranscriptSnapshot ClearTranscript() {
        lock (_transcriptSync) {
            TransferettoSshShellTranscriptSnapshot snapshot = new() {
                Entries = _transcriptEntries.ToArray(),
                CapturedEntryCount = _transcriptEntries.Count,
                DroppedEntryCount = _droppedTranscriptEntryCount,
                TotalCharacterCount = _transcriptCharacterCount
            };

            _transcriptEntries.Clear();
            _transcriptCharacterCount = 0;
            _droppedTranscriptEntryCount = 0;
            return snapshot;
        }
    }

    private void TrimTranscript() {
        while (_transcriptEntries.Count > MaxTranscriptEntries || _transcriptCharacterCount > MaxTranscriptCharacters) {
            TransferettoSshShellTranscriptEntry removedEntry = _transcriptEntries[0];
            _transcriptEntries.RemoveAt(0);
            _transcriptCharacterCount -= removedEntry.Text.Length;
            _droppedTranscriptEntryCount++;
        }
    }

    private static string NormalizeTranscriptText(string text, int maxTranscriptCharacters) {
        if (text.Length <= maxTranscriptCharacters) {
            return text;
        }

        int startIndex = text.Length - maxTranscriptCharacters;
        return text.Substring(startIndex, maxTranscriptCharacters);
    }
    /// <summary>
    /// Releases resources held by the SSH session.
    /// </summary>

    public void Dispose() {
        ShellStream.Dispose();
    }
}
