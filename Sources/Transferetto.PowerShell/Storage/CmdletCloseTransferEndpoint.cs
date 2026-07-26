using System;
using System.Management.Automation;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Disposes a Transferetto endpoint and its owned provider client.</para>
/// <example>
///   <code>Close-TransferEndpoint -Endpoint $s3</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Close, "TransferEndpoint")]
public sealed class CmdletCloseTransferEndpoint : PSCmdlet {
    /// <summary>Gets or sets the endpoint to dispose.</summary>
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        if (Endpoint is IDisposable disposable) {
            disposable.Dispose();
        }
    }
}
