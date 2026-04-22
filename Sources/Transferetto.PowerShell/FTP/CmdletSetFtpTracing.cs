using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPTracing cmdlet.
/// </summary>

[Cmdlet("Set", "FTPTracing")]
public sealed class CmdletSetFtpTracing : PSCmdlet
{
	/// <summary>
	/// Gets or sets a value indicating whether enable.
	/// </summary>
	[Parameter]
	public SwitchParameter Enable { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether disable.
	/// </summary>

	[Parameter]
	public SwitchParameter Disable { get; set; }
	/// <summary>
	/// Gets or sets the show Password.
	/// </summary>

	[Parameter]
	public SwitchParameter ShowPassword { get; set; }
	/// <summary>
	/// Gets or sets the hide User Name.
	/// </summary>

	[Parameter]
	public SwitchParameter HideUserName { get; set; }
	/// <summary>
	/// Gets or sets the hide IP.
	/// </summary>

	[Parameter]
	public SwitchParameter HideIP { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (!Enable.IsPresent && !Disable.IsPresent)
		{
			WriteWarning("Please specify either -Enable or -Disable.");
			return;
		}
		if (Disable.IsPresent)
		{
			TransferettoRuntimeSettings.FtpTraceOptions = null;
			return;
		}
		TransferettoRuntimeSettings.FtpTraceOptions = new TransferettoFtpTraceOptions
		{
			LogToConsole = true,
			LogUserName = !HideUserName.IsPresent,
			LogPassword = ShowPassword.IsPresent,
			LogHost = !HideIP.IsPresent
		};
	}
}

