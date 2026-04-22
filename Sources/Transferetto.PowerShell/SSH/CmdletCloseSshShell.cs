using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Closes an interactive SSH shell session.</para>
/// <para type="description">Stops the shell stream created by <c>New-SSHShell</c> while leaving the parent SSH connection available for other commands or for creating a replacement shell later.</para>
/// <example>
///   <para>Close a shell session once interactive work is finished.</para>
///   <code>Close-SSHShell -ShellSession $shell</code>
/// </example>
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
