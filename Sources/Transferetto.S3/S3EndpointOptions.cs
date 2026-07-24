namespace Transferetto.S3;

/// <summary>
/// Configures an Amazon S3 or S3-compatible transfer endpoint.
/// </summary>
public sealed class S3EndpointOptions {
    /// <summary>Gets or sets the bucket containing endpoint items.</summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional key prefix that scopes the endpoint.</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the AWS region or S3-compatible signing region.</summary>
    public string? Region { get; set; }

    /// <summary>Gets or sets a custom S3-compatible service URL.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Gets or sets whether path-style addressing is required.</summary>
    public bool ForcePathStyle { get; set; }

    /// <summary>Gets or sets an explicit access key identifier.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Gets or sets an explicit secret access key.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Gets or sets an optional session token for temporary credentials.</summary>
    public string? SessionToken { get; set; }
}
