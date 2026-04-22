using System;
using System.Collections.Generic;
using System.Management.Automation;
using FluentFTP;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Lists files and directories from an FTP or FTPS session.</para>
/// <para type="description">Returns Transferetto remote item objects for a target FTP path, with optional FluentFTP listing flags for recursive, force-listing, or link-aware enumeration scenarios.</para>
/// <example>
///   <para>List the current remote working directory.</para>
///   <code>Get-FTPList -Client $ftp</code>
/// </example>
/// <example>
///   <para>List a specific remote directory with explicit listing options.</para>
///   <code>Get-FTPList -Client $ftp -Path '/pub/example' -Options AllFiles</code>
/// </example>
/// </summary>

[Cmdlet("Get", "FTPList")]
public sealed class CmdletGetFtpList : PSCmdlet
{
	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	[Alias(new string[] { "FtpPath" })]
	[Parameter]
	public string? Path { get; set; }
	/// <summary>
	/// Gets or sets the options.
	/// </summary>

	[Parameter]
	public FtpListOption Options { get; set; }
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>

	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoFtpSession? Client { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (Client == null)
		{
			return;
		}
		try
		{
			bool flag = base.MyInvocation.BoundParameters.ContainsKey("Options");
			IReadOnlyList<TransferettoRemoteItem> ftpListing = TransferettoClient.GetFtpListing(Client, Path!, flag ? new FtpListOption?(Options) : ((FtpListOption?)null));
			WriteObject(ftpListing, enumerateCollection: true);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "GetFtpListFailed", ErrorCategory.ReadError, Path ?? Client.Host));
		}
	}
}
