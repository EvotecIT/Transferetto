# Transferetto - File and object transfer for .NET and PowerShell

<p align="center">
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/v/Transferetto.svg?style=flat-square" alt="PowerShell Gallery version"></a>
  <a href="https://www.nuget.org/packages/Transferetto"><img src="https://img.shields.io/nuget/v/Transferetto.svg?style=flat-square" alt="NuGet version"></a>
  <a href="https://github.com/EvotecIT/Transferetto/actions/workflows/test-dotnet.yml"><img src="https://github.com/EvotecIT/Transferetto/actions/workflows/test-dotnet.yml/badge.svg" alt=".NET tests"></a>
  <a href="https://github.com/EvotecIT/Transferetto/actions/workflows/test-powershell.yml"><img src="https://github.com/EvotecIT/Transferetto/actions/workflows/test-powershell.yml/badge.svg" alt="PowerShell tests"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/license/EvotecIT/Transferetto.svg?style=flat-square" alt="MIT license"></a>
</p>

Transferetto is a reusable transfer toolkit for PowerShell and .NET. It handles FTP, FTPS, SFTP, SCP, FXP, SSH, Amazon S3 and S3-compatible storage, Azure Blob Storage, and local or mounted filesystems without making every script or application learn a different transfer model.

Use the PowerShell module for automation and administration, or reference the focused .NET packages when building the transfer workflow into an application.

## At a glance

| Area | What Transferetto provides |
| --- | --- |
| FTP and FTPS | Uploads, downloads, directory operations, synchronization, streams, checksums, metadata, progress, proxies, auto-detection, and certificate policies |
| SFTP and SCP | File and directory transfers, SFTP content and stream access, permissions, timestamps, links, progress, and SSH proxy support |
| FXP | Direct FTP server-to-server file and directory transfers with preflight checks and progress |
| SSH | Commands, interactive shells, prompt-aware workflows, transcripts, expect-style steps, and local or remote tunnels |
| Object storage | One endpoint contract for Amazon S3, S3-compatible services, and Azure Blob Storage |
| Filesystems | A provider-neutral endpoint for local paths and mounted storage in .NET |
| Transfer controls | Explicit overwrite behavior, dry-run synchronization, cancellation, progress, metadata handling, and structured results |
| Integrity | Streaming SHA-256 receipts for provider-neutral endpoint copies |
| Security | FTPS certificate chain, pinning, TOFU, and known-certificate policies; SSH host-key pinning, TOFU, and known-hosts policies |

## Install

### PowerShell

```powershell
Install-Module Transferetto -Scope CurrentUser
```

### .NET

Install the umbrella package for every provider, or choose only the focused packages your application uses:

```shell
dotnet add package Transferetto
dotnet add package Transferetto.Protocols
dotnet add package Transferetto.Core
dotnet add package Transferetto.S3
dotnet add package Transferetto.AzureBlob
```

`Transferetto` is the convenience package and depends on all supported providers. `Transferetto.Protocols` owns the FTP, FTPS, SFTP, SCP, FXP, and SSH APIs. Every provider uses `Transferetto.Core`, which can also be referenced directly for the endpoint contract and filesystem transfers.

## PowerShell examples

### Upload a file with SFTP

Connections are reusable across listing, transfer, metadata, and directory commands.

```powershell
$sftp = Connect-SFTP `
    -Server 'sftp.example.com' `
    -Credential (Get-Credential) `
    -HostKeyPolicy TrustOnFirstUse

Send-SFTPFile `
    -SftpClient $sftp `
    -LocalPath '.\release.zip' `
    -RemotePath '/incoming/release.zip' `
    -AllowOverride `
    -ShowProgress

Get-SFTPItem -SftpClient $sftp -Path '/incoming/release.zip'
Disconnect-SFTP -SftpClient $sftp
```

For unattended systems, use an expected host-key fingerprint or a managed known-hosts file instead of accepting any host key.

### Preview and run a directory synchronization

FTP and SFTP synchronization can update or mirror a remote directory. Start with `-DryRun` when deletions or replacements are possible.

```powershell
$sftp = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)

$preview = Sync-SFTPDirectory `
    -SftpClient $sftp `
    -LocalPath '.\publish' `
    -RemotePath '/sites/app' `
    -Mode Mirror `
    -Include '*.html', '*.css', '*.js' `
    -Exclude 'archive/*' `
    -DryRun

$preview | Format-Table Action, RelativePath, Message

Sync-SFTPDirectory `
    -SftpClient $sftp `
    -LocalPath '.\publish' `
    -RemotePath '/sites/app' `
    -Mode Mirror `
    -Include '*.html', '*.css', '*.js' `
    -Exclude 'archive/*' `
    -ShowProgress

Disconnect-SFTP -SftpClient $sftp
```

### Transfer through FTP or FTPS

The same FTP command family covers unencrypted FTP and encrypted FTPS. Certificate validation stays explicit.

```powershell
$ftp = Connect-FTP `
    -Server 'ftps.example.com' `
    -Credential (Get-Credential) `
    -EncryptionMode Explicit `
    -CertificatePolicy PolicyChain

Send-FTPDirectory `
    -Client $ftp `
    -LocalPath '.\outgoing' `
    -RemotePath '/incoming' `
    -FolderSyncMode Update `
    -RemoteExists Overwrite `
    -ShowProgress

Get-FTPList -Client $ftp -Path '/incoming'
Disconnect-FTP -Client $ftp
```

Transferetto also supports certificate thumbprint pinning, trust on first use, known-certificate stores, connection auto-detection, proxies, rate limits, and direct FXP transfers between FTP servers.

### Run an SSH command

SSH sessions can run normal commands, open prompt-aware shells, retain transcripts, or host local and remote tunnels.

```powershell
$ssh = Connect-SSH `
    -Server 'linux.example.com' `
    -Credential (Get-Credential) `
    -HostKeyPolicy TrustOnFirstUse

$result = Send-SSHCommand `
    -SSHClient $ssh `
    -Command {
        'uname -a'
        'systemctl is-active nginx'
    }

$result | Format-List
Disconnect-SSH -SshClient $ssh
```

### Copy from S3 to Azure Blob Storage

Object-storage commands use one endpoint contract, so listing, upload, download, delete, and cross-provider copy keep the same shape. `New-Transfer*Endpoint` creates a reusable endpoint object; it does not create a bucket, container, or network session.

```powershell
$s3 = New-TransferS3Endpoint `
    -BucketName 'evidence' `
    -Region 'eu-central-1' `
    -Prefix 'incoming'

$blob = New-TransferAzureBlobEndpoint `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix 'archive'

$receipt = Copy-TransferItem `
    -SourceEndpoint $s3 `
    -SourcePath 'server01/latest.json' `
    -DestinationEndpoint $blob `
    -DestinationPath 'server01/latest.json' `
    -WriteMode FailIfExists `
    -ShowProgress

$receipt | Format-List

Get-TransferChildItem -Endpoint $blob -Path 'server01/' -Recurse

Close-TransferEndpoint -Endpoint $s3
Close-TransferEndpoint -Endpoint $blob
```

The receipt records the source and destination, transferred byte count, timestamps, provider entity tags, and a SHA-256 digest calculated while the content is streamed. The storage providers perform data-plane operations only; bucket creation, container creation, role assignment, and account administration remain infrastructure concerns.

## .NET examples

### Transfer a file over FTPS

The `TransferettoClient` API provides synchronous and asynchronous operations over reusable sessions.

```csharp
using System.Collections.Generic;
using System.Net;
using FluentFTP;
using Transferetto;

using TransferettoFtpSession ftp = TransferettoClient.ConnectFtp(
    new TransferettoFtpConnectionOptions {
        Server = "ftps.example.com",
        Credential = new NetworkCredential("user", "password"),
        EncryptionMode = new[] { FtpEncryptionMode.Explicit },
        CertificatePolicy = TransferettoFtpCertificatePolicy.PolicyChain
    });

IReadOnlyList<TransferettoTransferResult> results =
    await TransferettoClient.UploadFtpFilesAsync(
        ftp,
        "/incoming",
        new[] { "release.zip" },
        localFiles: null,
        remoteExists: FtpRemoteExists.Overwrite,
        createRemoteDirectory: true);
```

The same client exposes SFTP, SCP, FXP, SSH command, shell, tunnel, stream, and directory-synchronization APIs.

### Stream between endpoint providers

`Transferetto.Core` copies data between any two `ITransferEndpoint` implementations without loading the complete item into memory.

```csharp
using Transferetto.Core;

TransferReceipt receipt = await TransferEngine.CopyAsync(
    sourceEndpoint,
    "incoming/report.json",
    destinationEndpoint,
    "archive/report.json",
    new TransferCopyOptions {
        WriteOptions = new TransferWriteOptions {
            Mode = TransferWriteMode.FailIfExists
        }
    },
    cancellationToken);
```

Use `FileSystemTransferEndpoint` for a local or mounted-filesystem side of the copy. `Transferetto.Protocols` provides `FtpTransferEndpoint` and `SftpTransferEndpoint`; `Transferetto.S3` and `Transferetto.AzureBlob` provide the object-storage endpoints.

In PowerShell, wrap an existing protocol session before using the provider-neutral transfer commands:

```powershell
$ftpEndpoint = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential) |
    New-TransferFtpEndpoint -Prefix 'incoming' -OwnSession
$sftpEndpoint = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential) |
    New-TransferSftpEndpoint -Prefix 'archive' -OwnSession

Copy-TransferItem -SourceEndpoint $ftpEndpoint -SourcePath 'report.csv' `
    -DestinationEndpoint $sftpEndpoint -DestinationPath 'report.csv'
```

## Packages

| Package | Purpose |
| --- | --- |
| [`Transferetto`](https://www.nuget.org/packages/Transferetto) | Umbrella package that installs Core and every supported provider |
| [`Transferetto.Core`](https://www.nuget.org/packages/Transferetto.Core) | Provider-neutral endpoint contracts, streaming copy engine, filesystem endpoint, progress, integrity receipts, and metadata rules |
| [`Transferetto.Protocols`](https://www.nuget.org/packages/Transferetto.Protocols) | FTP, FTPS, SFTP, SCP, FXP, SSH, synchronization, streams, and protocol endpoint adapters |
| [`Transferetto.S3`](https://www.nuget.org/packages/Transferetto.S3) | Amazon S3 and S3-compatible object-storage provider |
| [`Transferetto.AzureBlob`](https://www.nuget.org/packages/Transferetto.AzureBlob) | Azure Blob Storage provider |
| [`Transferetto`](https://www.powershellgallery.com/packages/Transferetto) on PowerShell Gallery | One PowerShell command surface over the protocol and storage assemblies |

The umbrella and provider packages include focused package READMEs, while this repository README documents the complete toolkit.

## More examples

The [Examples directory](https://github.com/EvotecIT/Transferetto/tree/master/Examples) contains focused scripts for:

- FTP and FTPS connection discovery, proxies, certificate policies, progress, metadata, streams, and directory transfers
- SFTP host-key policies, files, directories, content, streams, permissions, timestamps, links, and progress
- SCP file and directory transfers
- FXP preflight and direct server-to-server transfers
- SSH commands, interactive shells, expect-style workflows, transcripts, and tunnels
- FTP and SFTP directory synchronization
- S3, Azure Blob Storage, and cross-provider object copies

The [protocol capability audit](https://github.com/EvotecIT/Transferetto/blob/master/Docs/ProtocolGapAudit.md) gives a deeper view of the implemented protocol surface.

Repository changes become available from NuGet and PowerShell Gallery after the corresponding packages are published. Check the installed package or module version when trying a recently added API or command.

## License

Transferetto is licensed under the [MIT License](https://github.com/EvotecIT/Transferetto/blob/master/LICENSE).
