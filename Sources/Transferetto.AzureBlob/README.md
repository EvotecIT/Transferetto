# Transferetto.AzureBlob

`Transferetto.AzureBlob` exposes one Azure Blob container prefix through the `Transferetto.Core` endpoint contract.

```csharp
using Azure.Identity;
using Transferetto.AzureBlob;

AzureBlobTransferEndpoint endpoint = new(
    new Uri("https://account.blob.core.windows.net/evidence"),
    new DefaultAzureCredential(),
    "servers");
```

The provider accepts a connection string, container SAS, shared-key credential, `TokenCredential`, or caller-owned `BlobContainerClient`.

Blob names and prefixes containing backslashes are rejected because Azure Blob Storage treats them as forward slashes. A fully resolved blob name beginning or ending with `/` is also rejected before an object operation because the Azure SDK would remove those slashes and address a different blob. Listing prefixes may end in `/`. Interior double slashes are preserved, including an endpoint-relative `/latest.txt` under prefix `scope/`, which addresses `scope//latest.txt`. Unsupported names fail explicitly instead of selecting another blob for a read, write, or delete.

`TransferEngine.CopyResumableAsync` uses staged blocks and a durable checkpoint to continue a large upload after cancellation. It rechecks available blocks and source content before committing. Azure expires uncommitted blocks; the engine restages any that are missing. `TransferCopyOptions.PreferServerSideCopy` uses an eligible signed source URI for a same-service copy and returns no streaming SHA-256 digest.

It performs blob data-plane operations only. Container creation, role assignment, lifecycle rules, and storage-account administration stay with infrastructure tooling.
