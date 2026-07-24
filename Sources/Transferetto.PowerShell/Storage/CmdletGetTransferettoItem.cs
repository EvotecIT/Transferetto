using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Gets or lists items from any Transferetto endpoint.</para>
/// <example>
///   <para>Inspect one object.</para>
///   <code>Get-TransferettoItem -Endpoint $s3 -Path evidence.json</code>
/// </example>
/// <example>
///   <para>List all objects beneath a prefix.</para>
///   <code>Get-TransferettoItem -Endpoint $blob -Path incoming/ -List -Recurse</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Get, "TransferettoItem")]
[OutputType(typeof(TransferItem))]
public sealed class CmdletGetTransferettoItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the source endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the item path or listing prefix.</summary>
    [Parameter(Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets whether items beneath the path are listed.</summary>
    [Parameter]
    public SwitchParameter List { get; set; }

    /// <summary>Gets or sets whether listing descends recursively.</summary>
    [Parameter]
    public SwitchParameter Recurse { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null) {
            return;
        }
        try {
            if (List.IsPresent) {
                WriteObject(await Endpoint.ListAsync(Path, Recurse.IsPresent, CancelToken).ConfigureAwait(false), true);
            } else {
                if (string.IsNullOrWhiteSpace(Path)) {
                    throw new ArgumentException("Path is required unless -List is used.", nameof(Path));
                }
                TransferItem? item = await Endpoint.GetItemAsync(Path, CancelToken).ConfigureAwait(false);
                if (item != null) {
                    WriteObject(item);
                }
            }
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "GetTransferettoItemFailed", ErrorCategory.ReadError, Path));
        }
    }
}
