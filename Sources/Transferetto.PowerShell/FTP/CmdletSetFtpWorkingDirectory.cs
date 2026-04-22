using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPWorkingDirectory cmdlet.
/// </summary>

[Cmdlet("Set", "FTPWorkingDirectory")]
public sealed class CmdletSetFtpWorkingDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
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
		if (Client == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoClient.SetFtpWorkingDirectory(Client, Path!);
			if (PassThru.IsPresent)
			{
				WriteObject(Client);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpWorkingDirectoryFailed", ErrorCategory.InvalidOperation, Path));
		}
	}
}

