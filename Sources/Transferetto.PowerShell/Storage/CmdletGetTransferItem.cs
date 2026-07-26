using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Gets one item from any Transferetto endpoint.</para>
/// <example>
///   <code>Get-TransferItem -Endpoint $s3 -Path evidence.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Get, "TransferItem")]
[OutputType(typeof(TransferItem))]
public sealed class CmdletGetTransferItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the source endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the item path.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null) {
            return;
        }
        try {
            TransferItem? item = await Endpoint.GetItemAsync(Path, CancelToken).ConfigureAwait(false);
            if (item != null) {
                WriteObject(item);
            }
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "GetTransferItemFailed", ErrorCategory.ReadError, Path));
        }
    }
}
