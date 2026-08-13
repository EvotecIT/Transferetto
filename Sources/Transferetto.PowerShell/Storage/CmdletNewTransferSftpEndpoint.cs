using System.Management.Automation;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Wraps a connected SFTP session as a Transferetto endpoint.</para>
/// <para type="description">Reuses a session created by Connect-SFTP so provider-neutral commands such as Copy-TransferItem can stream data between SFTP and other Transferetto providers.</para>
/// <example>
///   <code>$endpoint = $sftp | New-TransferSftpEndpoint -Prefix incoming</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.New, "TransferSftpEndpoint")]
[OutputType(typeof(SftpTransferEndpoint))]
public sealed class CmdletNewTransferSftpEndpoint : PSCmdlet {
    /// <summary>Gets or sets the connected SFTP session.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public TransferettoSftpSession? SftpSession { get; set; }

    /// <summary>Gets or sets the endpoint-relative remote path prefix.</summary>
    [Parameter]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets whether closing the endpoint also disposes the wrapped session.</summary>
    [Parameter]
    public SwitchParameter OwnSession { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SftpTransferEndpoint(SftpSession!, Prefix, OwnSession.IsPresent));
    }
}
