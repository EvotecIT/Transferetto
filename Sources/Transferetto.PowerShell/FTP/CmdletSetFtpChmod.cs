using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Sets POSIX-style permissions for a remote FTP item.</para>
/// <para type="description">Supports both octal-style integer permission values and explicit owner/group/other permission flags, depending on which representation is more convenient for the caller.</para>
/// <example>
///   <para>Set permissions by octal-style integer value.</para>
///   <code>Set-FTPChmod -Client $ftp -RemotePath '/wwwroot/index.html' -Permissions 644</code>
/// </example>
/// <example>
///   <para>Set permissions by explicit owner, group, and other values.</para>
///   <code>Set-FTPChmod -Client $ftp -RemotePath '/wwwroot/deploy.sh' -Owner Read,Write,Execute -Group Read,Execute -Other Read,Execute</code>
/// </example>
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
