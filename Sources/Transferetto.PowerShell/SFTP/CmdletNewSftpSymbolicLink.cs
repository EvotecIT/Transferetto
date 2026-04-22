using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the New-SFTPSymbolicLink cmdlet.
/// </summary>

[Cmdlet("New", "SFTPSymbolicLink")]
public sealed class CmdletNewSftpSymbolicLink : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the target Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? TargetPath { get; set; }
	/// <summary>
	/// Gets or sets the link Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LinkPath { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(TargetPath) || string.IsNullOrWhiteSpace(LinkPath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.CreateSftpSymbolicLink(SftpClient, TargetPath!, LinkPath!);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "NewSftpSymbolicLinkFailed", ErrorCategory.WriteError, LinkPath));
		}
	}
}

