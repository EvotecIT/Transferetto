using System;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// Implements the Compare-FTPFile cmdlet.
/// </summary>

[Cmdlet("Compare", "FTPFile")]
public sealed class CmdletCompareFtpFile : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true)]
	public TransferettoFtpSession? Client { get; set; }
	/// <summary>
	/// Gets or sets the local Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? LocalPath { get; set; }
	/// <summary>
	/// Gets or sets the remote Path.
	/// </summary>

	[Parameter(Mandatory = true)]
	public string? RemotePath { get; set; }
	/// <summary>
	/// Gets or sets the compare Option.
	/// </summary>

	[Parameter]
	public FtpCompareOption CompareOption { get; set; } = FtpCompareOption.Auto;

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null || string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
		{
			return;
		}
		try
		{
			WriteObject(TransferettoClient.CompareFtpFile(Client, LocalPath!, RemotePath!, CompareOption));
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "CompareFtpFileFailed", ErrorCategory.InvalidData, RemotePath));
		}
	}
}

