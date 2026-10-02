using System;
using System.Threading;

namespace Transferetto;
/// <summary>
/// Provides shared options for reading from an interactive SSH shell session.
/// </summary>

public sealed class TransferettoSshShellReadOptions {
    /// <summary>
    /// Gets or sets the maximum output characters retained by a shell read. Null retains all output;
    /// exceeding a configured limit throws IOException after reporting the received chunk to OutputProgress.
    /// </summary>
    public int? MaxCapturedCharacters { get; set; }
    /// <summary>
    /// Gets or sets a token used to cancel a shell read or wait operation.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    /// <summary>
    /// Gets or sets the progressive output sink used by shell read operations.
    /// </summary>

    public IProgress<TransferettoSshShellOutputChunk>? OutputProgress { get; set; }
    /// <summary>
    /// Gets or sets the poll interval used when waiting for additional shell output.
    /// </summary>

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(50);
}
