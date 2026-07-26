using System;
using System.Management.Automation;
using System.Security;
using Transferetto.S3;

namespace Transferetto.PowerShell;

/// <summary>
/// <para type="synopsis">Creates an Amazon S3 or S3-compatible Transferetto endpoint.</para>
/// <para type="description">Uses the AWS default credential chain unless an explicit access-key credential is supplied. Custom endpoints support S3-compatible services such as MinIO, Cloudflare R2, and Backblaze B2.</para>
/// <example>
///   <para>Connect with the AWS default credential chain.</para>
///   <code>$s3 = Connect-TransferettoS3 -BucketName evidence -Region eu-central-1 -Prefix company-a</code>
/// </example>
/// <example>
///   <para>Connect to an S3-compatible endpoint with explicit credentials.</para>
///   <code>$s3 = Connect-TransferettoS3 -BucketName evidence -ServiceUrl https://storage.example.com -Credential $credential -ForcePathStyle</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommunications.Connect, "TransferettoS3")]
[OutputType(typeof(S3TransferEndpoint))]
public sealed class CmdletConnectTransferettoS3 : PSCmdlet {
    /// <summary>Gets or sets the bucket name.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Gets or sets the endpoint-relative key prefix.</summary>
    [Parameter]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the AWS or signing region.</summary>
    [Parameter]
    public string? Region { get; set; }

    /// <summary>Gets or sets a custom S3-compatible service URL.</summary>
    [Parameter]
    public Uri? ServiceUrl { get; set; }

    /// <summary>Gets or sets an access-key credential whose username is the access key identifier.</summary>
    [Parameter]
    public PSCredential? Credential { get; set; }

    /// <summary>Gets or sets a session token for temporary credentials.</summary>
    [Parameter]
    public SecureString? SessionToken { get; set; }

    /// <summary>Gets or sets whether path-style bucket addressing is required.</summary>
    [Parameter]
    public SwitchParameter ForcePathStyle { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        S3EndpointOptions options = new() {
            BucketName = BucketName,
            Prefix = Prefix,
            Region = Region,
            ServiceUrl = ServiceUrl?.AbsoluteUri,
            ForcePathStyle = ForcePathStyle.IsPresent,
            AccessKeyId = Credential?.UserName,
            SecretAccessKey = Credential == null ? null : SecureStringValue.SecretFrom(Credential),
            SessionToken = SecureStringValue.RevealOptional(SessionToken)
        };
        WriteObject(new S3TransferEndpoint(options));
    }
}
