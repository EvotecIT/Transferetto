using System;
using System.Management.Automation;
using System.Text;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Reads the full content of a remote SFTP file as text or bytes.</para>
/// <para type="description">Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, with support for text decoding or raw byte retrieval.</para>
/// <example>
///   <para>Read a remote text file by using UTF-8 decoding.</para>
///   <code>Get-SFTPContent -SftpClient $sftp -Path '/srv/app/.env'</code>
/// </example>
/// <example>
///   <para>Read a remote file as raw bytes.</para>
///   <code>Get-SFTPContent -SftpClient $sftp -Path '/srv/app/logo.png' -AsByteArray</code>
/// </example>
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
