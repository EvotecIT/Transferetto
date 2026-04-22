using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Clear-SSHShellTranscript cmdlet.
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

