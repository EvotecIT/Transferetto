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

Provider-specific metadata remains visible on inspected items. Automatic cross-provider copies carry only metadata names accepted by every supported object provider; explicit destination metadata is validated rather than silently discarded.

`FileSystemTransferEndpoint` is intended for a root controlled by the caller's security context. It rejects symbolic links and reparse points observed while resolving a path, but it is not an operating-system sandbox against another process that can concurrently replace path components. Do not use a privileged process with a root writable by less-trusted identities.

File replacements preserve the destination's Unix permission bits before writing staged content. On Unix, the netstandard2.0 assembly requires a runtime with `File.GetUnixFileMode` and `File.SetUnixFileMode` for replacements; older runtimes reject the operation rather than broadening access. Windows replacements use the filesystem's atomic replacement operation.
