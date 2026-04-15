using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-SSHShellTranscript cmdlet.
/// </summary>

[Cmdlet("Get", "SSHShellTranscript")]
public sealed class CmdletGetSshShellTranscript : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the last.
	/// </summary>

	[Parameter]
	public int Last { get; set; }
	/// <summary>
	/// Gets or sets the as Text.
	/// </summary>

	[Parameter]
	public SwitchParameter AsText { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TransferettoSshShellTranscriptSnapshot sshShellTranscript = TransferettoClient.GetSshShellTranscript(ShellSession, (base.MyInvocation.BoundParameters.ContainsKey("Last") && Last > 0) ? new int?(Last) : ((int?)null));
			if (AsText.IsPresent)
			{
				foreach (TransferettoSshShellTranscriptEntry entry in sshShellTranscript.Entries)
				{
					WriteObject($"[{entry.TimestampUtc:O}] [{entry.Direction}]{Environment.NewLine}{entry.Text}");
				}
				if (sshShellTranscript.IsTruncated)
				{
					WriteObject($"[Transcript] Dropped {sshShellTranscript.DroppedEntryCount} older entries.");
				}
			}
			else
			{
				WriteObject(sshShellTranscript);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSshShellTranscriptFailed", ErrorCategory.ReadError, ShellSession.Host));
		}
	}
}

