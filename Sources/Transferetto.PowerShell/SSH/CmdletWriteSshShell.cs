using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Writes text into an interactive SSH shell session.</para>
/// <para type="description">Sends raw text or line-based input to an existing shell stream, with optional newline suppression and pass-through support so command composition can stay in PowerShell pipelines.</para>
/// <example>
///   <para>Send a command line into an interactive shell.</para>
///   <code>Write-SSHShell -ShellSession $shell -Text 'pwd'</code>
/// </example>
/// <example>
///   <para>Type a partial command without pressing Enter yet.</para>
///   <code>Write-SSHShell -ShellSession $shell -Text 'sudo systemctl restart nginx' -NoNewLine</code>
/// </example>
/// </summary>

[Alias(new string[] { "Send-SSHShell" })]
[Cmdlet("Write", "SSHShell")]
public sealed class CmdletWriteSshShell : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the text.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Text { get; set; }
	/// <summary>
	/// Gets or sets the no New Line.
	/// </summary>

	[Parameter]
	public SwitchParameter NoNewLine { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null || Text == null)
		{
			return;
		}
		try
		{
			TransferettoClient.WriteSshShell(ShellSession, Text, !NoNewLine.IsPresent);
			if (PassThru.IsPresent)
			{
				WriteObject(ShellSession);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "WriteSshShellFailed", ErrorCategory.WriteError, ShellSession.Host));
		}
	}
}
