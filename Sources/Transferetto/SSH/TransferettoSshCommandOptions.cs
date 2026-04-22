using System;
using System.Text;
using System.Threading;

namespace Transferetto;
/// <summary>
/// Provides shared options for non-interactive SSH command execution.
/// </summary>

public sealed class TransferettoSshCommandOptions {
    /// <summary>
    /// Gets or sets a token used to cancel a running SSH command.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    /// <summary>
    /// Gets or sets the timeout applied to the remote command.
    /// </summary>

    public TimeSpan? CommandTimeout { get; set; }
    /// <summary>
    /// Gets or sets the text encoding used for command output streams.
    /// </summary>

    public Encoding? OutputEncoding { get; set; }
    /// <summary>
    /// Gets or sets the progressive output sink used by command execution.
    /// </summary>

    public IProgress<TransferettoSshCommandOutputChunk>? OutputProgress { get; set; }
}
