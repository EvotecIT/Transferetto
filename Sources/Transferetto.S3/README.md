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

The provider performs object data-plane operations only. Bucket creation, access policy, lifecycle, replication, and account administration stay with infrastructure tooling.
