using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Preflights whether an FXP transfer can run between two FTP/FTPS sessions.</para>
/// <para type="description">Evaluates the requested source, destination, transfer kind, and optional destination-directory creation rules before a full FXP transfer is attempted.</para>
/// <example>
///   <para>Preflight a direct server-to-server file transfer.</para>
///   <code>Test-FXPTransfer -Client $source -SourcePath '/pub/site.zip' -DestinationClient $destination -DestinationPath '/incoming/site.zip'</code>
/// </example>
/// <example>
///   <para>Preflight a directory transfer where the destination path may need to be created.</para>
///   <code>Test-FXPTransfer -Client $source -SourcePath '/pub/site' -DestinationClient $destination -DestinationPath '/mirror/site' -TransferKind Directory -CreateRemoteDirectory</code>
/// </example>
/// </summary>

[Cmdlet("Test", "FXPTransfer")]
public sealed class CmdletTestFxpTransfer : PSCmdlet
{
	/// <summary>
	/// Gets or sets the source session object used by the cmdlet.
	/// </summary>
	[Alias(new string[] { "SourceClient" })]
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the source path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? SourcePath { get; set; }
	/// <summary>
	/// Gets or sets the destination session object used by the cmdlet.
	/// </summary>

	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? DestinationClient { get; set; }
	/// <summary>
	/// Gets or sets the destination path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }
	/// <summary>
	/// Gets or sets the FXP transfer kind.
	/// </summary>

	[Parameter]
	public TransferettoFxpTransferKind TransferKind { get; set; } = TransferettoFxpTransferKind.File;
	/// <summary>
	/// Gets or sets a value indicating whether a missing destination parent can be created by the transfer.
	/// </summary>

	[Parameter]
	public SwitchParameter CreateRemoteDirectory { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || DestinationClient == null || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.TestFxpTransfer(Client, SourcePath!, DestinationClient, DestinationPath!, TransferKind, CreateRemoteDirectory.IsPresent));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestFxpTransferFailed", ErrorCategory.InvalidOperation, DestinationPath));
		}
	}
}
