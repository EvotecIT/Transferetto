using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Retrieves the in-memory transcript captured for an interactive SSH shell session.</para>
/// <para type="description">Returns either a structured transcript snapshot or plain text entries, with optional trimming to the newest entries so long-running interactive sessions can be inspected without exporting everything.</para>
/// <example>
///   <para>Return the full structured transcript snapshot.</para>
///   <code>Get-SSHShellTranscript -ShellSession $shell</code>
/// </example>
/// <example>
///   <para>Return the last 20 transcript entries as formatted text.</para>
///   <code>Get-SSHShellTranscript -ShellSession $shell -Last 20 -AsText</code>
/// </example>
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
