using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SFTPContent cmdlet.
/// </summary>

[Cmdlet("Set", "SFTPContent", DefaultParameterSetName = "Text")]
public sealed class CmdletSetSftpContent : PSCmdlet
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
	/// Gets or sets the value.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Text")]
	public string? Value { get; set; }
	/// <summary>
	/// Gets or sets the encoding.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public Encoding? Encoding { get; set; }
	/// <summary>
	/// Gets or sets the append.
	/// </summary>

	[Parameter(ParameterSetName = "Text")]
	public SwitchParameter Append { get; set; }
	/// <summary>
	/// Gets or sets the byte Content.
	/// </summary>

	[Parameter(Mandatory = true, ParameterSetName = "Bytes")]
	public byte[]? ByteContent { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			TransferettoOperationResult sendToPipeline = ((base.ParameterSetName == "Bytes") ? TransferettoClient.WriteSftpBytes(SftpClient, Path!, ByteContent!) : TransferettoClient.WriteSftpText(SftpClient, Path!, Value ?? string.Empty, Append.IsPresent, Encoding));
			WriteObject(sendToPipeline);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpContentFailed", ErrorCategory.WriteError, Path));
		}
	}
}

