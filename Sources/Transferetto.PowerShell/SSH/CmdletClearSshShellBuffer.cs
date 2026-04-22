using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Clears buffered unread output from an interactive SSH shell session.</para>
/// <para type="description">Returns the buffered text by default so callers can inspect or discard it explicitly, or suppress output when they simply want a clean shell buffer before continuing an automation flow.</para>
/// <example>
///   <para>Flush buffered shell output and capture what was removed.</para>
///   <code>$pending = Clear-SSHShellBuffer -ShellSession $shell</code>
/// </example>
/// <example>
///   <para>Discard any buffered output before sending the next command.</para>
///   <code>Clear-SSHShellBuffer -ShellSession $shell -Suppress</code>
/// </example>
/// </summary>

[Cmdlet("Clear", "SSHShellBuffer")]
public sealed class CmdletClearSshShellBuffer : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			string sendToPipeline = TransferettoClient.ClearSshShellBuffer(ShellSession);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ClearSshShellBufferFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}
