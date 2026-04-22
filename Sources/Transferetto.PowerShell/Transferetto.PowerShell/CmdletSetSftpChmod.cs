using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SFTPChmod cmdlet.
/// </summary>

[Cmdlet("Set", "SFTPChmod", DefaultParameterSetName = "ByString")]
public sealed class CmdletSetSftpChmod : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ParameterSetName = "ByString")]
	[Parameter(Mandatory = true, ParameterSetName = "ByDigits")]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByString")]
	[Parameter(Mandatory = true, ParameterSetName = "ByDigits")]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the permissions.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByString")]
	public string? Permissions { get; set; }
	/// <summary>
	/// Gets or sets the owner.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByDigits")]
	public int Owner { get; set; }
	/// <summary>
	/// Gets or sets the group.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByDigits")]
	public int Group { get; set; }
	/// <summary>
	/// Gets or sets the other.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByDigits")]
	public int Other { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = ((base.ParameterSetName == "ByDigits") ? TransferettoClient.SetSftpChmod(SftpClient, Path!, Owner, Group, Other) : TransferettoClient.SetSftpChmod(SftpClient, Path!, Permissions!));
			if (PassThru.IsPresent)
			{
				WriteObject(TransferettoClient.GetSftpAttributes(SftpClient, Path!));
			}
			else
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpChmodFailed", ErrorCategory.WriteError, Path));
		}
	}
}

