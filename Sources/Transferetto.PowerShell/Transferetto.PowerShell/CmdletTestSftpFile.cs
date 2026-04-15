using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Test-SFTPFile cmdlet.
/// </summary>

[Cmdlet("Test", "SFTPFile")]
public sealed class CmdletTestSftpFile : PSCmdlet
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

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.TestSftpFile(SftpClient, Path!));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "TestSftpFileFailed", ErrorCategory.ReadError, Path));
		}
	}
}

