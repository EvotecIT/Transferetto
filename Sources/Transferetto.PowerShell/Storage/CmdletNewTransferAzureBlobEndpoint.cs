using System;
using System.Management.Automation;
using System.Security;
using Azure;
using Azure.Identity;
using Azure.Storage;
using Transferetto.AzureBlob;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Creates an Azure Blob Transferetto endpoint.</para>
/// <para type="description">Connects through a protected connection string or a container URI, including a container SAS URI. The endpoint performs blob data-plane operations and does not create or administer storage resources.</para>
/// <example>
///   <para>Connect with a protected connection string.</para>
///   <code>$blob = New-TransferAzureBlobEndpoint -ConnectionString $connectionString -ContainerName evidence -Prefix company-a</code>
/// </example>
/// <example>
///   <para>Connect with the default Azure credential chain.</para>
///   <code>$blob = New-TransferAzureBlobEndpoint -ContainerUri https://account.blob.core.windows.net/evidence -UseDefaultCredential</code>
/// </example>
/// <example>
///   <para>Keep a SAS token separate from the safe container URI.</para>
///   <code>$blob = New-TransferAzureBlobEndpoint -ContainerUri https://account.blob.core.windows.net/evidence -SasToken $sasToken</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.New, "TransferAzureBlobEndpoint", DefaultParameterSetName = "ConnectionString")]
[OutputType(typeof(AzureBlobTransferEndpoint))]
public sealed class CmdletNewTransferAzureBlobEndpoint : PSCmdlet {
    /// <summary>Gets or sets a protected Azure Storage connection string.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "ConnectionString")]
    public SecureString? ConnectionString { get; set; }

    /// <summary>Gets or sets the blob container name used with a connection string.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "ConnectionString")]
    public string? ContainerName { get; set; }

    /// <summary>Gets or sets a container URI, optionally containing a SAS token.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "ContainerUri")]
    [Parameter(Mandatory = true, ParameterSetName = "SasToken")]
    [Parameter(Mandatory = true, ParameterSetName = "SharedKey")]
    [Parameter(Mandatory = true, ParameterSetName = "DefaultCredential")]
    public Uri? ContainerUri { get; set; }

    /// <summary>Gets or sets a separately protected SAS token.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "SasToken")]
    public SecureString? SasToken { get; set; }

    /// <summary>Gets or sets an account credential whose username is the account name and password is the shared key.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "SharedKey")]
    public PSCredential? SharedKeyCredential { get; set; }

    /// <summary>Gets or sets whether the Azure default credential chain is used.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "DefaultCredential")]
    public SwitchParameter UseDefaultCredential { get; set; }

    /// <summary>Gets or sets the endpoint-relative blob prefix.</summary>
    [Parameter]
    public string Prefix { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override void ProcessRecord() {
        AzureBlobTransferEndpoint endpoint;
        switch (ParameterSetName) {
            case "ContainerUri":
                endpoint = new AzureBlobTransferEndpoint(ContainerUri!, Prefix);
                break;
            case "SasToken":
                endpoint = new AzureBlobTransferEndpoint(
                    ContainerUri!,
                    new AzureSasCredential(SecureStringValue.Reveal(SasToken!)),
                    Prefix);
                break;
            case "SharedKey":
                endpoint = new AzureBlobTransferEndpoint(
                    ContainerUri!,
                    new StorageSharedKeyCredential(
                        SharedKeyCredential!.UserName,
                        SecureStringValue.SecretFrom(SharedKeyCredential)),
                    Prefix);
                break;
            case "DefaultCredential":
                endpoint = new AzureBlobTransferEndpoint(
                    ContainerUri!,
                    new DefaultAzureCredential(),
                    Prefix);
                break;
            default:
                endpoint = new AzureBlobTransferEndpoint(
                    SecureStringValue.Reveal(ConnectionString!),
                    ContainerName!,
                    Prefix);
                break;
        }
        WriteObject(endpoint);
    }
}
