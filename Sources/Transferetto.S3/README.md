# Transferetto.S3

`Transferetto.S3` exposes one Amazon S3 or S3-compatible bucket prefix through the `Transferetto.Core` endpoint contract.

```csharp
using Transferetto.S3;

using S3TransferEndpoint endpoint = new(new S3EndpointOptions {
    BucketName = "evidence",
    Region = "eu-central-1",
    Prefix = "servers"
});
```

The default AWS credential chain is used when explicit credentials are not supplied. Custom service URLs support S3-compatible providers and must use HTTPS unless the endpoint is loopback.

Uploads with an unknown length or content beyond the 5 GiB single-request limit use multipart upload. Failed or cancelled uploads are aborted, and no-overwrite modes are enforced again when the multipart upload is completed.

Metadata returned by S3 is preserved even when its names are provider-specific. Transferetto automatically carries only portable metadata names into another provider; metadata supplied explicitly for a destination remains validated.

The provider performs object data-plane operations only. Bucket creation, access policy, lifecycle, replication, and account administration stay with infrastructure tooling.
