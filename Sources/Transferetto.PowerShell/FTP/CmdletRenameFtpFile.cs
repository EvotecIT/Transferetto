using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Renames a remote FTP file or relocates it to a new path.</para>
/// <para type="description">Performs a server-side rename for a remote FTP item, which is useful for finalizing staged uploads or rotating files in place.</para>
/// <example>
///   <para>Rename a staged file to its final name.</para>
///   <code>Rename-FTPFile -Client $ftp -Path '/incoming/site.tmp' -DestinationPath '/incoming/site.zip'</code>
/// </example>
/// </summary>

[Cmdlet("Rename", "FTPFile")]
public sealed class CmdletRenameFtpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the destination Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? DestinationPath { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(Path) || string.IsNullOrWhiteSpace(DestinationPath))
		{
			return;
		}
		try
		{
			TransferettoClient.RenameFtpFile(Client, Path!, DestinationPath!);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "RenameFtpFileFailed", ErrorCategory.WriteError, DestinationPath));
		}
	}
}
