using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Resizes the virtual terminal backing an interactive SSH shell session.</para>
/// <para type="description">Updates terminal rows, columns, and optional pixel dimensions so interactive programs such as editors, pagers, or full-screen tools render correctly in the remote shell.</para>
/// <example>
///   <para>Resize the shell to a standard 120x40 terminal.</para>
///   <code>Set-SSHShellSize -ShellSession $shell -Columns 120 -Rows 40</code>
/// </example>
/// <example>
///   <para>Resize the shell and continue with the same shell session.</para>
///   <code>$shell = Set-SSHShellSize -ShellSession $shell -Columns 160 -Rows 48 -PassThru</code>
/// </example>
/// </summary>

[Alias(new string[] { "Resize-SSHShell" })]
[Cmdlet("Set", "SSHShellSize")]
public sealed class CmdletSetSshShellSize : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
	/// <summary>
	/// Gets or sets the columns.
	/// </summary>

	[Parameter(Mandatory = true)]
	public uint Columns { get; set; }
	/// <summary>
	/// Gets or sets the rows.
	/// </summary>

	[Parameter(Mandatory = true)]
	public uint Rows { get; set; }
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
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ShellSession == null)
		{
			return;
		}
		try
		{
			TransferettoClient.ResizeSshShell(ShellSession, Columns, Rows, Width, Height);
			if (PassThru.IsPresent)
			{
				WriteObject(ShellSession);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "ResizeSshShellFailed", ErrorCategory.InvalidOperation, ShellSession.Host));
		}
	}
}
