using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the New-SSHShell cmdlet.
/// </summary>

[Cmdlet("New", "SSHShell")]
public sealed class CmdletNewSshShell : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoSshSession? SshClient { get; set; }
	/// <summary>
	/// Gets or sets the terminal Name.
	/// </summary>

	[Parameter]
	public string TerminalName { get; set; } = "xterm-256color";
	/// <summary>
	/// Gets or sets the columns.
	/// </summary>

	[Parameter]
	public uint Columns { get; set; } = 120u;
	/// <summary>
	/// Gets or sets the rows.
	/// </summary>

	[Parameter]
	public uint Rows { get; set; } = 40u;
	/// <summary>
	/// Gets or sets the width.
	/// </summary>

	[Parameter]
	public uint Width { get; set; }
	/// <summary>
	/// Gets or sets the height.
	/// </summary>

	[Parameter]
	public uint Height { get; set; }
	/// <summary>
	/// Gets or sets the buffer Size.
	/// </summary>

	[Parameter]
	public int BufferSize { get; set; } = 4096;
	/// <summary>
	/// Gets or sets the no Terminal.
	/// </summary>

	[Parameter]
	public SwitchParameter NoTerminal { get; set; }
	/// <summary>
	/// Gets or sets the prompt Pattern.
	/// </summary>

	[Parameter]
	public string? PromptPattern { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SshClient == null)
		{
			return;
		}
		try
		{
			TransferettoSshShellOptions options = new TransferettoSshShellOptions
			{
				TerminalName = TerminalName,
				Columns = Columns,
				Rows = Rows,
				Width = Width,
				Height = Height,
				BufferSize = BufferSize,
				NoTerminal = NoTerminal.IsPresent,
				PromptPattern = PromptPattern
			};
			WriteObject(TransferettoClient.CreateSshShell(SshClient, options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CreateSshShellFailed", ErrorCategory.OpenError, SshClient.Host));
		}
	}
}

