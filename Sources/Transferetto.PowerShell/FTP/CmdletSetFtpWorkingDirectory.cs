using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Changes the working directory for an FTP or FTPS session.</para>
/// <para type="description">Updates the session’s active FTP path so later relative operations run against the intended remote location.</para>
/// <example>
///   <para>Change the remote working directory.</para>
///   <code>Set-FTPWorkingDirectory -Client $ftp -Path '/wwwroot/releases'</code>
/// </example>
/// <example>
///   <para>Change the directory and keep using the same session in the pipeline.</para>
///   <code>$ftp = Set-FTPWorkingDirectory -Client $ftp -Path '/wwwroot/releases' -PassThru</code>
/// </example>
/// </summary>

[Cmdlet("Set", "FTPWorkingDirectory")]
public sealed class CmdletSetFtpWorkingDirectory : PSCmdlet
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
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoClient.SetFtpWorkingDirectory(Client, Path!);
			if (PassThru.IsPresent)
			{
				WriteObject(Client);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetFtpWorkingDirectoryFailed", ErrorCategory.InvalidOperation, Path));
		}
	}
}
