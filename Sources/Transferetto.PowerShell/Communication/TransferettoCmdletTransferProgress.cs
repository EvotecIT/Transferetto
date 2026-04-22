using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;

/// <summary>
/// Reports shared transfer progress to the PowerShell progress stream.
/// </summary>
internal sealed class TransferettoCmdletTransferProgress : IProgress<TransferettoTransferProgress> {
    private readonly AsyncPSCmdlet _cmdlet;

    /// <summary>
    /// Initializes a new progress reporter for the specified cmdlet.
    /// </summary>
    internal TransferettoCmdletTransferProgress(AsyncPSCmdlet cmdlet) {
        _cmdlet = cmdlet;
    }

    /// <inheritdoc />
    public void Report(TransferettoTransferProgress value) {
        int percentComplete = value.PercentComplete ?? -1;
        string activity = $"{value.Protocol} {value.Direction}";
        string status = value.TotalBytes.HasValue
            ? $"{value.BytesTransferred} of {value.TotalBytes.Value} bytes"
            : $"{value.BytesTransferred} bytes";

        _cmdlet.WriteProgress(new ProgressRecord(0, activity, status) {
            PercentComplete = percentComplete,
            CurrentOperation = value.RemotePath ?? value.LocalPath
        });
    }
}
