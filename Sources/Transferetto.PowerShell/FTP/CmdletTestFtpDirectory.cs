using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Checks whether a remote FTP directory exists.</para>
/// <para type="description">Returns a Boolean-like existence result for a remote FTP directory path, which is useful before create, remove, or sync operations.</para>
/// <example>
///   <para>Check whether a remote directory exists.</para>
///   <code>Test-FTPDirectory -Client $ftp -RemotePath '/wwwroot/releases'</code>
/// </example>
/// </summary>

[Cmdlet("Test", "FTPDirectory")]
public sealed class CmdletTestFtpDirectory : PSCmdlet
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.TestFtpDirectory(Client, RemotePath!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestFtpDirectoryFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}
