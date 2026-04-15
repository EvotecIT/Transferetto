using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-SFTPContent cmdlet.
/// </summary>

[Cmdlet("Get", "SFTPContent", DefaultParameterSetName = "Text")]
public sealed class CmdletGetSftpContent : PSCmdlet
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
	/// Gets or sets the encoding.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public Encoding? Encoding { get; set; }
	/// <summary>
	/// Gets or sets the as Byte Array.
	/// </summary>

	[Parameter(ParameterSetName = "Bytes")]
	public SwitchParameter AsByteArray { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			if (base.ParameterSetName == "Bytes")
			{
				WriteObject(TransferettoClient.ReadSftpBytes(SftpClient, Path!));
			}
			else
			{
				WriteObject(TransferettoClient.ReadSftpText(SftpClient, Path!, Encoding));
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetSftpContentFailed", ErrorCategory.ReadError, Path));
		}
	}
}

