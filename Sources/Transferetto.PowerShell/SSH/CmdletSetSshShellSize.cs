using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SSHShellSize cmdlet.
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

