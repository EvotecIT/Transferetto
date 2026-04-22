using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Close-SSHShell cmdlet.
/// </summary>

[Cmdlet("Close", "SSHShell")]
public sealed class CmdletCloseSshShell : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.CloseSshShell(ShellSession);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CloseSshShellFailed", ErrorCategory.CloseError, ShellSession.Host));
		}
	}
}

