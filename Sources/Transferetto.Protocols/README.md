# Transferetto.Protocols

`Transferetto.Protocols` contains the FTP, FTPS, FXP, SFTP, SCP, and SSH implementations used by Transferetto.

The package provides the existing `TransferettoClient` protocol API together with `FtpTransferEndpoint` and `SftpTransferEndpoint` adapters for the provider-neutral `Transferetto.Core` copy engine.

Use the protocol API for protocol-specific operations such as FTP synchronization, SSH commands, shells, and tunnels. Use the endpoint adapters with `TransferEngine.CopyAsync` when data must move between FTP/SFTP, filesystems, S3, or Azure Blob Storage through one streaming contract.

Local directory transfers and synchronization reject symbolic links and Windows junctions instead of following them. Remote names must be representable on the destination filesystem; Windows downloads reject backslash traversal, alternate data streams, and reserved device names. The selected local root must be controlled by the caller, since path checks cannot prevent another process from replacing directories during a transfer.

Download synchronization compares paths without case on Windows by default, which keeps mirror cleanup from deleting a file through a differently cased name. Set `TransferettoSyncOptions.PathComparison` to `Ordinal` when the destination is a case-sensitive Windows directory, or to `OrdinalIgnoreCase` for another case-insensitive filesystem.

Known-host and FTPS certificate stores serialize updates across processes and replace the store atomically. Malformed stores fail closed; repair or replace the affected store explicitly. SSH SHA-256 pins preserve the case-sensitive Base64 digest, while MD5 hexadecimal pins accept either hex case.

SFTP file transfers stage replacement content before committing it. The asynchronous file APIs use asynchronous SFTP I/O, close remote handles on cancellation, and report upload progress as source bytes read. Shell readers check their deadline even during continuous output. Use `TransferettoSshShellReadOptions.MaxCapturedCharacters` to bound retained output and `OutputProgress` to consume arriving chunks; exceeding the capture limit throws `IOException`.
