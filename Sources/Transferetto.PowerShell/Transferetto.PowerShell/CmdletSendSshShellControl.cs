using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Send-SSHShellControl cmdlet.
/// </summary>

[Cmdlet("Send", "SSHShellControl")]
public sealed class CmdletSendSshShellControl : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the key.
	/// </summary>

	[Parameter(Mandatory = true)]
	public TransferettoSshShellControlKey Key { get; set; }
	/// <summary>
	/// Gets or sets the repeat.
	/// </summary>

	[Parameter]
	public int Repeat { get; set; } = 1;
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.SendSshShellControl(ShellSession, Key, Repeat);
			if (PassThru.IsPresent)
			{
				WriteObject(ShellSession);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SendSshShellControlFailed", ErrorCategory.WriteError, ShellSession.Host));
		}
	}
}

