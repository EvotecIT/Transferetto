# Transferetto

`Transferetto` is the umbrella package for the provider-neutral transfer engine and the supported protocol and object-storage providers.

Install a narrower package when an application needs only one capability:

- `Transferetto.Core` for endpoint contracts, filesystem transfers, streaming copies, progress, and receipts.
- `Transferetto.Protocols` for FTP, FTPS, FXP, SFTP, SCP, and SSH.
- `Transferetto.S3` for Amazon S3 and S3-compatible storage.
- `Transferetto.AzureBlob` for Azure Blob Storage.

The umbrella package references all four packages for applications that want the complete Transferetto surface.
