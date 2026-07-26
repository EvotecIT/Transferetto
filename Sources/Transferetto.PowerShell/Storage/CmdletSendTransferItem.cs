using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Uploads a local file to any writable Transferetto endpoint.</para>
/// <example>
///   <para>Upload signed evidence without replacing an existing object.</para>
///   <code>Send-TransferItem -Endpoint $s3 -LocalPath .\zone.txevidence.json -Path zones/a/latest.txevidence.json</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommunications.Send, "TransferItem")]
[OutputType(typeof(TransferReceipt))]
public sealed class CmdletSendTransferItem : AsyncPSCmdlet {
    /// <summary>Gets or sets the destination endpoint.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public ITransferEndpoint? Endpoint { get; set; }

    /// <summary>Gets or sets the local source file.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the destination item path. The source filename is used by default.</summary>
    [Parameter(Position = 2)]
    public string? Path { get; set; }

    /// <summary>Gets or sets destination collision behavior.</summary>
    [Parameter]
    public TransferWriteMode WriteMode { get; set; } = TransferWriteMode.FailIfExists;

    /// <summary>Gets or sets the destination content type.</summary>
    [Parameter]
    public string? ContentType { get; set; }

    /// <summary>Gets or sets provider metadata.</summary>
    [Parameter]
    public Hashtable? Metadata { get; set; }

    /// <summary>Gets or sets whether transfer progress is displayed.</summary>
    [Parameter]
    public SwitchParameter ShowProgress { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (Endpoint == null) {
            return;
        }
        try {
            FileInfo sourceFile = new(GetUnresolvedProviderPathFromPSPath(LocalPath));
            if (!sourceFile.Exists) {
                throw new FileNotFoundException("The local source file does not exist.", sourceFile.FullName);
            }
            string destinationPath = string.IsNullOrWhiteSpace(Path) ? sourceFile.Name : Path!;
            FileSystemTransferEndpoint source = new(sourceFile.DirectoryName!);
            TransferCopyOptions options = CreateOptions();
            TransferReceipt receipt = await TransferEngine.CopyAsync(
                source,
                sourceFile.Name,
                Endpoint,
                destinationPath,
                options,
                CancelToken).ConfigureAwait(false);
            WriteObject(receipt);
        } catch (Exception exception) {
            WriteError(new ErrorRecord(exception, "SendTransferItemFailed", ErrorCategory.WriteError, Path));
        }
    }

    private TransferCopyOptions CreateOptions() {
        TransferWriteOptions writeOptions = new() {
            Mode = WriteMode,
            ContentType = ContentType
        };
        if (Metadata != null) {
            foreach (DictionaryEntry pair in Metadata) {
                if (pair.Key != null && pair.Value != null) {
                    writeOptions.Metadata[Convert.ToString(pair.Key)!] = Convert.ToString(pair.Value)!;
                }
            }
        }
        return new TransferCopyOptions {
            WriteOptions = writeOptions,
            Progress = ShowProgress.IsPresent ? new TransferettoEndpointProgress(this) : null
        };
    }
}
