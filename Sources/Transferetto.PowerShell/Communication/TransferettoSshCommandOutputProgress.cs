using System;

namespace Transferetto.PowerShell;

/// <summary>
/// Reports progressive SSH command output to the PowerShell pipeline.
/// </summary>
internal sealed class TransferettoSshCommandOutputProgress : IProgress<TransferettoSshCommandOutputChunk> {
    private readonly AsyncPSCmdlet _cmdlet;

    /// <summary>
    /// Initializes a new SSH command output reporter for the specified cmdlet.
    /// </summary>
    internal TransferettoSshCommandOutputProgress(AsyncPSCmdlet cmdlet) {
        _cmdlet = cmdlet;
    }

    /// <inheritdoc />
    public void Report(TransferettoSshCommandOutputChunk value) {
        _cmdlet.WriteObject(value);
    }
}
