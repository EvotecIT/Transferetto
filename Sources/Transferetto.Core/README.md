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

Use `TransferCopyOptions.ExpectedSha256` to require a digest match before a destination commits staged content. Set `VerifyDestination` to read the committed item back; a failed readback reports an error after the write. `TransferEngine.CopyBatchAsync` runs a bounded number of copies and returns each item in input order, including failures and items that did not start after fail-fast cancellation. Endpoints sharing an FTP or SFTP session are coordinated by session identity; streamed FTP copies require separate source and destination connections.

`TransferBatchOptions.CheckpointPath` records completed items so a restarted batch can reuse a result only when source identity and destination SHA-256 still match. `TransferEngine.CopyResumableAsync` checkpoints a single copy from an `ITransferRangeEndpoint` to a local file, S3 multipart upload, or Azure staged blocks. It verifies staged parts and the source before commit. Keep checkpoint files in a private directory and retain them after cancellation. A destination readback requires `VerifyDestination`; a failed readback reports an error after commit.

`PreferServerSideCopy` opts into an eligible S3 or Azure provider-side copy. That receipt has no streaming SHA-256 value. Request `ExpectedSha256` or `VerifyDestination` to use the streaming path instead.

Subscribe to the `Transferetto.Core` `ActivitySource` or `Meter` exposed by `TransferDiagnostics` for copy, protocol operation, and connection outcomes, byte counts, durations, and cleanup failures. Default telemetry includes schemes, operation names, and error types; it does not include paths, endpoint URLs, credentials, or metadata.

File replacements preserve the destination's Unix permission bits before writing staged content. On Unix, the netstandard2.0 assembly requires a runtime with `File.GetUnixFileMode` and `File.SetUnixFileMode` for replacements; older runtimes reject the operation rather than broadening access. Windows replacements use the filesystem's atomic replacement operation.
