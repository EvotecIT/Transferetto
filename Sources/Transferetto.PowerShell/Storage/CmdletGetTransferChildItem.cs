using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Lists items beneath a path on any Transferetto endpoint.</para>
/// <example>
///   <para>List all objects beneath a prefix.</para>
///   <code>Get-TransferChildItem -Endpoint $blob -Path incoming/ -Recurse</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Get, "TransferChildItem")]
[OutputType(typeof(TransferItem))]
public sealed class CmdletGetTransferChildItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the source endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the listing prefix. The endpoint root is used by default.</summary>
    [Parameter(Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets whether listing descends recursively.</summary>
    [Parameter]
    public SwitchParameter Recurse { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null) {
            return;
        }
        try {
            WriteObject(await Endpoint.ListAsync(Path, Recurse.IsPresent, CancelToken).ConfigureAwait(false), true);
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "GetTransferChildItemFailed", ErrorCategory.ReadError, Path));
        }
    }
}
