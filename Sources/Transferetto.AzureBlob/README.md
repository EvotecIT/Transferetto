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

It performs blob data-plane operations only. Container creation, role assignment, lifecycle rules, and storage-account administration stay with infrastructure tooling.
