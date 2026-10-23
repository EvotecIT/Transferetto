# Transferetto.Protocols

`Transferetto.Protocols` contains the FTP, FTPS, FXP, SFTP, SCP, and SSH implementations used by Transferetto.

The package provides the existing `TransferettoClient` protocol API together with `FtpTransferEndpoint` and `SftpTransferEndpoint` adapters for the provider-neutral `Transferetto.Core` copy engine.

Use the protocol API for protocol-specific operations such as FTP synchronization, SSH commands, shells, and tunnels. Use the endpoint adapters with `TransferEngine.CopyAsync` when data must move between FTP/SFTP, filesystems, S3, or Azure Blob Storage through one streaming contract.

Local directory transfers and synchronization reject symbolic links and Windows junctions instead of following them. Remote names must be representable on the destination filesystem; Windows downloads reject backslash traversal, alternate data streams, and reserved device names. The selected local root must be controlled by the caller, since path checks cannot prevent another process from replacing directories during a transfer.

Filesystem synchronization resolves `TransferettoSyncOptions.PathComparison.Automatic` from the destination directory. Windows per-directory settings and macOS volume settings are queried without creating probe files. Other Unix filesystems use existing entries to detect case aliases, falling back to case-sensitive comparison when no evidence is available. Explicit comparison options affect planning, but cannot disable physical destination collision checks. Mirror plans reject a delete that addresses a file copied or retained through another spelling.

Recursive SFTP receive checks the remote manifest before writing and checks names again during transfer. SCP checks incoming members as they arrive. Distinct remote names that map to one destination, including case-equivalent directory names, fail instead of overwriting or silently skipping another source member. SCP can already have committed earlier members when a later collision is received. FTP manifest traversal lists literal directory names, including `*` and `?` on compatible filesystems, and observes cancellation between entries and descents.

Endpoint synchronization preserves the source's exact name when inspecting and reading it. Literal backslashes are preserved on Unix destinations and rejected before writes on Windows destinations. Unrepresentable destination names fail explicitly instead of becoming a different directory hierarchy.

Known-host and FTPS certificate stores serialize updates across processes and replace the store atomically. Malformed stores fail closed; repair or replace the affected store explicitly. SSH SHA-256 pins preserve the case-sensitive Base64 digest, while MD5 hexadecimal pins accept either hex case.

SFTP file transfers stage replacement content before committing it. The asynchronous file APIs use asynchronous SFTP I/O, close remote handles on cancellation, and report upload progress as source bytes read. Shell readers check their deadline even during continuous output. Use `TransferettoSshShellReadOptions.MaxCapturedCharacters` to bound retained output and `OutputProgress` to consume arriving chunks; exceeding the capture limit throws `IOException`.
