# Transferetto.Protocols

`Transferetto.Protocols` contains the FTP, FTPS, FXP, SFTP, SCP, and SSH implementations used by Transferetto.

The package provides the existing `TransferettoClient` protocol API together with `FtpTransferEndpoint` and `SftpTransferEndpoint` adapters for the provider-neutral `Transferetto.Core` copy engine.

Use the protocol API for protocol-specific operations such as FTP synchronization, SSH commands, shells, and tunnels. Use the endpoint adapters with `TransferEngine.CopyAsync` when data must move between FTP/SFTP, filesystems, S3, or Azure Blob Storage through one streaming contract.

Local directory transfers and synchronization reject symbolic links and Windows junctions instead of following them. Remote names must be representable on the destination filesystem; Windows downloads reject backslash traversal, alternate data streams, and reserved device names. The selected local root must be controlled by the caller, since path checks cannot prevent another process from replacing directories during a transfer.

Filesystem synchronization resolves `TransferettoSyncOptions.PathComparison.Automatic` from the selected destination directory. Windows per-directory settings, macOS volume settings, and Linux filesystem/per-directory casefold settings are queried without creating probe files. Unknown Unix filesystems use existing entries to detect case aliases and conservatively treat names without distinguishing evidence as case-insensitive. Receive registries keep those comparison decisions for the operation. Explicit comparison options affect planning, but cannot disable destination collision checks. Mirror plans reject a delete that addresses a file copied or retained through another spelling.

Recursive SFTP receive checks the remote manifest before writing and checks names again during transfer. SCP checks incoming members as they arrive. Distinct remote names that map to one destination, including case-equivalent directory names, fail instead of overwriting or silently skipping another source member. FTP manifest traversal lists literal directory names, including `*` and `?` on compatible filesystems, and observes cancellation between entries and descents.

SCP directory downloads stage incoming content privately on the destination filesystem and publish it only after the complete directory stream and remote command succeed. Rejected names, incomplete content, cancellation, and transport failures discard the staged download. Publication preserves unrelated existing files, gives new files their destination directory's access rules, and rolls back replacements and newly created directories if a local write fails. If rollback itself fails, the error identifies the retained recovery directory instead of deleting its backups. This requires temporary space for incoming files, replaced originals, and one adjacent publishing copy; it does not provide crash recovery or isolation from concurrent local writers. SFTP and FTP directory APIs continue to report incremental per-item results.

Endpoint synchronization preserves the source's exact name when inspecting and reading it. Literal backslashes are preserved on Unix destinations and rejected before writes on Windows destinations. Unrepresentable destination names fail explicitly instead of becoming a different directory hierarchy.

Known-host and FTPS certificate stores serialize updates across processes and replace the store atomically. Malformed stores fail closed; repair or replace the affected store explicitly. SSH SHA-256 pins preserve the case-sensitive Base64 digest, while MD5 hexadecimal pins accept either hex case.

SFTP file transfers stage replacement content before committing it. The asynchronous file APIs use asynchronous SFTP I/O, close remote handles on cancellation, and report upload progress as source bytes read. Shell readers check their deadline even during continuous output. Use `TransferettoSshShellReadOptions.MaxCapturedCharacters` to bound retained output and `OutputProgress` to consume arriving chunks; exceeding the capture limit throws `IOException`.
