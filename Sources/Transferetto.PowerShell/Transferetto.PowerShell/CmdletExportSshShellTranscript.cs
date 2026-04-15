using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Export-SSHShellTranscript cmdlet.
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

