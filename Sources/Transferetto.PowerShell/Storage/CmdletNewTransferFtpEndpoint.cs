using System.Management.Automation;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Wraps a connected FTP or FTPS session as a Transferetto endpoint.</para>
/// <para type="description">Reuses a session created by Connect-FTP so provider-neutral commands such as Copy-TransferItem can stream data between FTP or FTPS and other Transferetto providers.</para>
/// <example>
///   <code>$endpoint = $ftp | New-TransferFtpEndpoint -Prefix incoming</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.New, "TransferFtpEndpoint")]
[OutputType(typeof(FtpTransferEndpoint))]
public sealed class CmdletNewTransferFtpEndpoint : PSCmdlet {
    /// <summary>Gets or sets the connected FTP or FTPS session.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public TransferettoFtpSession? FtpSession { get; set; }

    /// <summary>Gets or sets the endpoint-relative remote path prefix.</summary>
    [Parameter]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets whether closing the endpoint also disposes the wrapped session.</summary>
    [Parameter]
    public SwitchParameter OwnSession { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new FtpTransferEndpoint(FtpSession!, Prefix, OwnSession.IsPresent));
    }
}
