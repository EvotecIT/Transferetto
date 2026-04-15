using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Open-SFTPStream cmdlet.
/// </summary>

[Cmdlet("Open", "SFTPStream")]
public sealed class CmdletOpenSftpStream : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoSftpSession? SftpClient { get; set; }
	/// <summary>
	/// Gets or sets the path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the mode.
	/// </summary>

	[Parameter]
	public TransferettoSftpStreamMode Mode { get; set; } = TransferettoSftpStreamMode.Read;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.OpenSftpStream(SftpClient, Path!, Mode));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "OpenSftpStreamFailed", ErrorCategory.OpenError, Path));
		}
	}
}

