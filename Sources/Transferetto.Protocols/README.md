# Transferetto.Protocols

`Transferetto.Protocols` contains the FTP, FTPS, FXP, SFTP, SCP, and SSH implementations used by Transferetto.

The package provides the existing `TransferettoClient` protocol API together with `FtpTransferEndpoint` and `SftpTransferEndpoint` adapters for the provider-neutral `Transferetto.Core` copy engine.

Use the protocol API for protocol-specific operations such as FTP synchronization, SSH commands, shells, and tunnels. Use the endpoint adapters with `TransferEngine.CopyAsync` when data must move between FTP/SFTP, filesystems, S3, or Azure Blob Storage through one streaming contract.
