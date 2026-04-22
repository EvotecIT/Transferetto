using System;
using System.Management.Automation;

namespace Transferetto.PowerShell;
/// <summary>
/// <para type="synopsis">Disconnects an SCP session.</para>
/// <para type="description">Closes the reusable SCP session created by <c>Connect-SCP</c> so the underlying SSH transport is released cleanly after copy operations complete.</para>
/// <example>
///   <para>Close the current SCP session.</para>
///   <code>Disconnect-SCP -ScpClient $scp</code>
/// </example>
/// </summary>

[Cmdlet("Disconnect", "SCP")]
public sealed class CmdletDisconnectScp : PSCmdlet
{
	/// <summary>
	/// Gets or sets the session object used by the cmdlet.
	/// </summary>
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public TransferettoScpSession? ScpClient { get; set; }

	/// <inheritdoc/>
	protected override void ProcessRecord()
	{
		if (ScpClient == null)
		{
			return;
		}
		try
		{
			TransferettoClient.DisconnectScp(ScpClient);
		}
		catch (Exception exception)
		{
			WriteError(new ErrorRecord(exception, "DisconnectScpFailed", ErrorCategory.CloseError, ScpClient.Host));
		}
	}
}
