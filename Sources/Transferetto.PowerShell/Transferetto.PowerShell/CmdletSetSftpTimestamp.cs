using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Set-SFTPTimestamp cmdlet.
/// </summary>

[Cmdlet("Set", "SFTPTimestamp")]
public sealed class CmdletSetSftpTimestamp : PSCmdlet
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
	/// Gets or sets the last Access Time.
	/// </summary>

	[Parameter]
	public DateTime LastAccessTime { get; set; }
	/// <summary>
	/// Gets or sets the last Write Time.
	/// </summary>

	[Parameter]
	public DateTime LastWriteTime { get; set; }
	/// <summary>
	/// Gets or sets the utc.
	/// </summary>

	[Parameter]
	public SwitchParameter Utc { get; set; }
	/// <summary>
	/// Gets or sets the pass Thru.
	/// </summary>

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (SftpClient == null || string.IsNullOrWhiteSpace(Path))
		{
			return;
		}
		try
		{
			bool flag = base.MyInvocation.BoundParameters.ContainsKey("LastAccessTime");
			bool flag2 = base.MyInvocation.BoundParameters.ContainsKey("LastWriteTime");
			TransferettoOperationResult sendToPipeline = TransferettoClient.SetSftpTimestamp(SftpClient, Path!, flag ? new DateTime?(LastAccessTime) : ((DateTime?)null), flag2 ? new DateTime?(LastWriteTime) : ((DateTime?)null), Utc.IsPresent);
			if (PassThru.IsPresent)
			{
				WriteObject(TransferettoClient.GetSftpAttributes(SftpClient, Path!));
			}
			else
			{
				WriteObject(sendToPipeline);
			}
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "SetSftpTimestampFailed", ErrorCategory.WriteError, Path));
		}
	}
}

