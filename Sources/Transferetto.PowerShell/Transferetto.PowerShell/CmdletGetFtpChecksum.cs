using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Get-FTPChecksum cmdlet.
/// </summary>

[Cmdlet("Get", "FTPChecksum")]
public sealed class CmdletGetFtpChecksum : PSCmdlet
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
	/// Gets or sets the hash Algorithm.
	/// </summary>

	[Parameter]
	public FtpHashAlgorithm HashAlgorithm { get; set; } = FtpHashAlgorithm.MD5;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.GetFtpChecksum(Client, RemotePath!, HashAlgorithm));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpChecksumFailed", ErrorCategory.ReadError, RemotePath));
		}
	}
}

