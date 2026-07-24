# Transferetto.Core

`Transferetto.Core` contains the provider-neutral transfer contract used by Transferetto storage providers.

```csharp
TransferReceipt receipt = await TransferEngine.CopyAsync(
    sourceEndpoint,
    "incoming/evidence.json",
    destinationEndpoint,
    "archive/evidence.json",
    new TransferCopyOptions {
        WriteOptions = new TransferWriteOptions {
            Mode = TransferWriteMode.FailIfExists
        }
    },
    cancellationToken);
```

The engine streams content without loading the complete item into memory. A successful receipt includes the transferred byte count and a SHA-256 digest calculated during the copy.

Endpoints implement inspect, list, read, write, and delete through `ITransferEndpoint`. `FileSystemTransferEndpoint` provides the built-in local or mounted-filesystem implementation.

`FileSystemTransferEndpoint` is intended for a root controlled by the caller's security context. It rejects symbolic links and reparse points observed while resolving a path, but it is not an operating-system sandbox against another process that can concurrently replace path components. Do not use a privileged process with a root writable by less-trusted identities.
