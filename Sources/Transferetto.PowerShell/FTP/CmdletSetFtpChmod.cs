using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-FTPChmod cmdlet.
/// </summary>

[Cmdlet("Set", "FTPChmod", DefaultParameterSetName = "ByInt")]
public sealed class CmdletSetFtpChmod : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ParameterSetName = "ByInt")]
	[Parameter(Mandatory = true, ParameterSetName = "Explicit")]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByInt")]
	[Parameter(Mandatory = true, ParameterSetName = "Explicit")]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the permissions.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "ByInt")]
	public int Permissions { get; set; }
	/// <summary>
	/// Gets or sets the owner.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Explicit")]
	public FtpPermission Owner { get; set; }
	/// <summary>
	/// Gets or sets the group.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Explicit")]
	public FtpPermission Group { get; set; }
	/// <summary>
	/// Gets or sets the other.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Explicit")]
	public FtpPermission Other { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			if (base.ParameterSetName == "ByInt")
			{
				TransferettoClient.SetFtpChmod(Client, RemotePath!, Permissions!);
			}
			else
			{
				TransferettoClient.SetFtpChmod(Client, RemotePath!, Owner, Group, Other);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpChmodFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}

