using System;

namespace Transferetto.PowerShell;

/// <summary>
/// Reports progressive SSH shell output to the PowerShell pipeline.
/// </summary>
internal sealed class TransferettoSshShellOutputProgress : IProgress<TransferettoSshShellOutputChunk> {
    private readonly AsyncPSCmdlet _cmdlet;

    /// <summary>
    /// Initializes a new SSH shell output reporter for the specified cmdlet.
    /// </summary>
    internal TransferettoSshShellOutputProgress(AsyncPSCmdlet cmdlet) {
        _cmdlet = cmdlet;
    }

    /// <inheritdoc />
    public void Report(TransferettoSshShellOutputChunk value) {
        _cmdlet.WriteObject(value);
    }
}
