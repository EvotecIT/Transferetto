using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Sends control-key input to an interactive SSH shell session.</para>
/// <para type="description">Provides a safe way to send interrupt and navigation keys such as Ctrl+C or Ctrl+D without embedding terminal escape sequences directly into shell automation scripts.</para>
/// <example>
///   <para>Interrupt the currently running command.</para>
///   <code>Send-SSHShellControl -ShellSession $shell -Key CtrlC</code>
/// </example>
/// <example>
///   <para>Send the up-arrow twice to recall earlier history entries.</para>
///   <code>Send-SSHShellControl -ShellSession $shell -Key UpArrow -Repeat 2</code>
/// </example>
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
