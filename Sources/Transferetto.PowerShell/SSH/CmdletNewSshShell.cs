using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates a reusable interactive SSH shell session.</para>
/// <para type="description">Builds an interactive shell stream on top of an SSH session, with optional prompt presets or explicit prompt patterns, transcript support, and terminal sizing that can be reused by shell read, write, expect, and recipe cmdlets.</para>
/// <example>
///   <para>Create a standard Linux shell session using the built-in prompt preset.</para>
///   <code>$shell = New-SSHShell -SshClient $ssh -PromptPreset Linux</code>
/// </example>
/// <example>
///   <para>Create a wider shell session and supply an explicit prompt pattern.</para>
///   <code>$shell = New-SSHShell -SshClient $ssh -Columns 160 -Rows 50 -PromptPattern '(?m)^[^\r\n]*[#$]\s?$'</code>
/// </example>
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
	/// <summary>
	/// Gets or sets the reusable prompt preset applied when no explicit prompt pattern is supplied.
	/// </summary>

	[Parameter]
	public TransferettoSshShellPromptPreset PromptPreset { get; set; }

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
				PromptPattern = PromptPattern,
				PromptPreset = PromptPreset
			};
			WriteObject(TransferettoClient.CreateSshShell(SshClient, options));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CreateSshShellFailed", ErrorCategory.OpenError, SshClient.Host));
		}
	}
}
