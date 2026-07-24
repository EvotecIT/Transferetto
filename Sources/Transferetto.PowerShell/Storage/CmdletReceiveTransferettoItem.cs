using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Downloads an item from any readable Transferetto endpoint.</para>
/// <example>
///   <code>Receive-TransferettoItem -Endpoint $s3 -Path zones/a/latest.txevidence.json -LocalPath .\latest.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommunications.Receive, "TransferettoItem")]
[OutputType(typeof(TransferReceipt))]
public sealed class CmdletReceiveTransferettoItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the source endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the source item path.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets the local destination file.</summary>
    [Parameter(Mandatory = true, Position = 2)]
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets whether an existing local file is replaced.</summary>
    [Parameter]
    public SwitchParameter Force { get; set; }

    /// <summary>Gets or sets whether transfer progress is displayed.</summary>
    [Parameter]
    public SwitchParameter ShowProgress { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null) {
            return;
        }
        try {
            string destinationFullPath = GetUnresolvedProviderPathFromPSPath(LocalPath);
            string? destinationDirectory = System.IO.Path.GetDirectoryName(destinationFullPath);
            if (string.IsNullOrWhiteSpace(destinationDirectory)) {
                throw new ArgumentException("LocalPath must resolve to a file path.", nameof(LocalPath));
            }
            FileSystemTransferEndpoint destination = new(destinationDirectory);
            TransferReceipt receipt = await TransferEngine.CopyAsync(
                Endpoint,
                Path,
                destination,
                System.IO.Path.GetFileName(destinationFullPath),
                new TransferCopyOptions {
                    WriteOptions = new TransferWriteOptions {
                        Mode = Force.IsPresent ? TransferWriteMode.Overwrite : TransferWriteMode.FailIfExists
                    },
                    Progress = ShowProgress.IsPresent ? new TransferettoEndpointProgress(this) : null
                },
                CancelToken).ConfigureAwait(false);
            WriteObject(receipt);
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "ReceiveTransferettoItemFailed", ErrorCategory.ReadError, Path));
        }
    }
}
