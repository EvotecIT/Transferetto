using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Tests whether an item exists on any Transferetto endpoint.</para>
/// <example>
///   <code>Test-TransferItem -Endpoint $s3 -Path evidence.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, "TransferItem")]
[OutputType(typeof(bool))]
public sealed class CmdletTestTransferItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the item path.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        try {
            WriteObject(Endpoint != null && await Endpoint.GetItemAsync(Path, CancelToken).ConfigureAwait(false) != null);
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "TestTransferItemFailed", ErrorCategory.ReadError, Path));
        }
    }
}
