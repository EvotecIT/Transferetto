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

Blob names and prefixes preserve literal backslashes; `reports\latest.txt` and `reports/latest.txt` are different names. Leading-slash names and prefixes are rejected before an operation because the Azure SDK would remove that slash and address a different blob. Unsupported names are never silently rewritten for reads or deletes.

`TransferEngine.CopyResumableAsync` uses staged blocks and a durable checkpoint to continue a large upload after cancellation. It rechecks available blocks and source content before committing. Azure expires uncommitted blocks; the engine restages any that are missing. `TransferCopyOptions.PreferServerSideCopy` uses an eligible signed source URI for a same-service copy and returns no streaming SHA-256 digest.

It performs blob data-plane operations only. Container creation, role assignment, lifecycle rules, and storage-account administration stay with infrastructure tooling.
