using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Exports the interactive SSH shell transcript to a file.</para>
/// <para type="description">Writes the shell transcript to disk for troubleshooting or audit purposes, with optional append mode and support for exporting only the newest entries from a long-running shell session.</para>
/// <example>
///   <para>Export the entire transcript to a log file.</para>
///   <code>Export-SSHShellTranscript -ShellSession $shell -Path '.\transcripts\session.log'</code>
/// </example>
/// <example>
///   <para>Append only the most recent transcript entries to an existing file.</para>
///   <code>Export-SSHShellTranscript -ShellSession $shell -Path '.\transcripts\session.log' -Last 50 -Append</code>
/// </example>
/// </summary>

[Cmdlet("Export", "SSHShellTranscript")]
public sealed class CmdletExportSshShellTranscript : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the last.
	/// </summary>

	[Parameter]
	public int Last { get; set; }
	/// <summary>
	/// Gets or sets the append.
	/// </summary>

	[Parameter]
	public SwitchParameter Append { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.ExportSshShellTranscript(ShellSession, Path!, Append.IsPresent, (base.MyInvocation.BoundParameters.ContainsKey("Last") && Last > 0) ? new int?(Last) : ((int?)null)));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ExportSshShellTranscriptFailed", ErrorCategory.WriteError, Path));
		}
	}
}
