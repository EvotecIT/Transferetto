using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SFTPWorkingDirectory cmdlet.
/// </summary>

[Cmdlet("Set", "SFTPWorkingDirectory")]
public sealed class CmdletSetSftpWorkingDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoClient.SetSftpWorkingDirectory(SftpClient, Path!);
			if (PassThru.IsPresent)
			{
				WriteObject(SftpClient);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpWorkingDirectoryFailed", ErrorCategory.InvalidOperation, Path));
		}
	}
}

