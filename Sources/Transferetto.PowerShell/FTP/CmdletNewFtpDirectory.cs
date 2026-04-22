using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Creates a directory on an FTP or FTPS server.</para>
/// <para type="description">Creates a remote directory and can force parent creation when needed, returning a structured result unless output is intentionally suppressed.</para>
/// <example>
///   <para>Create a remote directory.</para>
///   <code>New-FTPDirectory -Client $ftp -RemotePath '/incoming/releases'</code>
/// </example>
/// <example>
///   <para>Create nested parent directories when they do not exist yet.</para>
///   <code>New-FTPDirectory -Client $ftp -RemotePath '/archive/2026/04/22' -Force</code>
/// </example>
/// </summary>

[Cmdlet("New", "FTPDirectory")]
public sealed class CmdletNewFtpDirectory : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the force.
	/// </summary>

	[Parameter]
	public SwitchParameter Force { get; set; }
	/// <summary>
	/// Gets or sets the suppress.
	/// </summary>

	[Parameter]
	public SwitchParameter Suppress { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = TransferettoClient.CreateFtpDirectory(Client, RemotePath!, Force.IsPresent);
			if (!Suppress.IsPresent)
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "NewFtpDirectoryFailed", ErrorCategory.WriteError, RemotePath));
		}
	}
}
