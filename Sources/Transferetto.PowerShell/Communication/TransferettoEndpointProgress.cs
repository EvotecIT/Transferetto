using System;
using System.Management.Automation;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// Reports provider-neutral endpoint transfer progress to PowerShell.
/// </summary>
internal sealed class TransferettoEndpointProgress : IProgress<TransferProgress> {
    private readonly AsyncPSCmdlet _cmdlet;

    internal TransferettoEndpointProgress(AsyncPSCmdlet cmdlet) {
        _cmdlet = cmdlet;
    }

    /// <inheritdoc />
    public void Report(TransferProgress value) {
        string status = value.TotalBytes.HasValue
            ? $"{value.BytesTransferred} of {value.TotalBytes.Value} bytes"
            : $"{value.BytesTransferred} bytes";
        _cmdlet.WriteProgress(new ProgressRecord(0, "Transferetto transfer", status) {
            PercentComplete = value.PercentComplete ?? -1,
            CurrentOperation = $"{value.SourcePath} -> {value.DestinationPath}"
        });
    }
}
