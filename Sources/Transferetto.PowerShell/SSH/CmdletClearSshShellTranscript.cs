using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Clears the in-memory transcript stored for an interactive SSH shell session.</para>
/// <para type="description">Returns the removed transcript snapshot by default so callers can archive it before clearing, or suppress output when they simply want to reset transcript state before the next automation step.</para>
/// <example>
///   <para>Clear the transcript and inspect the snapshot that was removed.</para>
///   <code>$previous = Clear-SSHShellTranscript -ShellSession $shell</code>
/// </example>
/// <example>
///   <para>Reset transcript capture without writing anything to the pipeline.</para>
///   <code>Clear-SSHShellTranscript -ShellSession $shell -Suppress</code>
/// </example>
/// </summary>

[Cmdlet("Clear", "SSHShellTranscript")]
public sealed class CmdletClearSshShellTranscript : PSCmdlet
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
			TransferettoSshShellTranscriptSnapshot sendToPipeline = TransferettoClient.ClearSshShellTranscript(ShellSession);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ClearSshShellTranscriptFailed", ErrorCategory.WriteError, ShellSession.Host));
		}
	}
}
