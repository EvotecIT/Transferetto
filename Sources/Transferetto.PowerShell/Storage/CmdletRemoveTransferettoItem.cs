using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Deletes an item from any writable Transferetto endpoint.</para>
/// <example>
///   <code>Remove-TransferettoItem -Endpoint $s3 -Path obsolete.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Remove, "TransferettoItem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[OutputType(typeof(bool))]
public sealed class CmdletRemoveTransferettoItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the item path.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null || !ShouldProcess($"{Endpoint.DisplayName}{Path}", "Delete transfer item")) {
            return;
        }
        try {
            WriteObject(await Endpoint.DeleteAsync(Path, CancelToken).ConfigureAwait(false));
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "RemoveTransferettoItemFailed", ErrorCategory.WriteError, Path));
        }
    }
}
