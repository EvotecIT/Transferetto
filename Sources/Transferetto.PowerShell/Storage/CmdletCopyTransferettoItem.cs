using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Streams an item between any two Transferetto endpoints.</para>
/// <para type="description">The content is relayed without loading the full item into memory. The returned receipt contains a provider-independent SHA-256 digest.</para>
/// <example>
///   <code>Copy-TransferettoItem -SourceEndpoint $s3 -SourcePath incoming/a.json -DestinationEndpoint $blob -DestinationPath archive/a.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Copy, "TransferettoItem")]
[OutputType(typeof(TransferReceipt))]
public sealed class CmdletCopyTransferettoItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the source endpoint.</summary>
    [Parameter(Mandatory = true)]
    public ITransferEndpoint? SourceEndpoint { get; set; }

    /// <summary>Gets or sets the source item path.</summary>
    [Parameter(Mandatory = true)]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the destination endpoint.</summary>
    [Parameter(Mandatory = true)]
    public ITransferEndpoint? DestinationEndpoint { get; set; }

    /// <summary>Gets or sets the destination item path.</summary>
    [Parameter(Mandatory = true)]
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>Gets or sets destination collision behavior.</summary>
    [Parameter]
    public TransferWriteMode WriteMode { get; set; } = TransferWriteMode.FailIfExists;

    /// <summary>Gets or sets whether transfer progress is displayed.</summary>
    [Parameter]
    public SwitchParameter ShowProgress { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (SourceEndpoint == null || DestinationEndpoint == null) {
            return;
        }
        try {
            TransferReceipt receipt = await TransferEngine.CopyAsync(
                SourceEndpoint,
                SourcePath,
                DestinationEndpoint,
                DestinationPath,
                new TransferCopyOptions {
                    WriteOptions = new TransferWriteOptions { Mode = WriteMode },
                    Progress = ShowProgress.IsPresent ? new TransferettoEndpointProgress(this) : null
                },
                CancelToken).ConfigureAwait(false);
            WriteObject(receipt);
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "CopyTransferettoItemFailed", ErrorCategory.WriteError, DestinationPath));
        }
    }
}
