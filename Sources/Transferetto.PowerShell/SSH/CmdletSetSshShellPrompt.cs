using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Updates the prompt detection settings for an interactive SSH shell session.</para>
/// <para type="description">Configures either an explicit prompt regex or a reusable prompt preset so later read, expect, and command cmdlets can synchronize against the correct shell prompt.</para>
/// <example>
///   <para>Switch the shell to use the built-in Linux prompt preset.</para>
///   <code>Set-SSHShellPrompt -ShellSession $shell -PromptPreset Linux</code>
/// </example>
/// <example>
///   <para>Set a custom prompt regex and continue with the same shell session.</para>
///   <code>$shell = Set-SSHShellPrompt -ShellSession $shell -PromptPattern '(?m)^deploy@web01:[^\\r\\n]*\\$\\s?$' -PassThru</code>
/// </example>
/// </summary>

[Cmdlet("Set", "SSHShellPrompt")]
public sealed class CmdletSetSshShellPrompt : PSCmdlet
{
	/// <summary>
	/// Gets or sets the shell Session.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSshShellSession? ShellSession { get; set; }
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
			TransferettoClient.SetSshShellPromptPattern(ShellSession, PromptPattern!, PromptPreset);
			if (PassThru.IsPresent)
			{
				WriteObject(ShellSession);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSshShellPromptFailed", ErrorCategory.InvalidOperation, ShellSession.Host));
		}
	}
}
