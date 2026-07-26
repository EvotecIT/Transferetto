# Transferetto

<p align="center">
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/v/Transferetto.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/Transferetto/actions/workflows/test-dotnet.yml"><img src="https://github.com/EvotecIT/Transferetto/actions/workflows/test-dotnet.yml/badge.svg"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/license/EvotecIT/Transferetto.svg?style=flat-square"></a>
</p>

Transferetto moves files and object data from one endpoint to another. The PowerShell module covers FTP, FTPS, SFTP, SCP, FXP, SSH, Amazon S3 and S3-compatible storage, Azure Blob Storage, and local or mounted filesystems.

The source tree includes the object-storage commands below. Repository changes are not available from PowerShell Gallery until a release is published, so check the installed module version when trying new commands.

## Install the published module

```powershell
Install-Module Transferetto -Scope CurrentUser
```

## Storage endpoints

Storage commands use the same endpoint contract, so upload, download, inspect, list, delete, and cross-provider copy do not need provider-specific implementations.

```powershell
# AWS default credential chain: environment, profiles, roles, or workload identity
$s3 = Connect-TransferettoS3 -BucketName evidence -Region eu-central-1 -Prefix servers

# Passwordless Azure authentication through DefaultAzureCredential
$blob = Connect-TransferettoAzureBlob `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix archive

Send-TransferettoItem `
    -Endpoint $s3 `
    -LocalPath '.\server01.txevidence.json' `
    -Path 'server01/2026-07-24.txevidence.json' `
    -ContentType 'application/json' `
    -Metadata @{ schema = 'testimo_evidence_v3' }

Copy-TransferettoItem `
    -SourceEndpoint $s3 `
    -SourcePath 'server01/2026-07-24.txevidence.json' `
    -DestinationEndpoint $blob `
    -DestinationPath 'server01/2026-07-24.txevidence.json'
```

Every completed copy returns a receipt with the source and destination, byte count, timestamps, provider entity tags, and a SHA-256 digest calculated while the content is streamed. `SkipIfExists`, `FailIfExists`, and `Overwrite` make collision behavior explicit.

For explicit credentials, use `PSCredential` or `SecureString` values:

```powershell
$s3Credential = Get-Credential -Message 'Username = access key; password = secret key'
$s3 = Connect-TransferettoS3 `
    -BucketName evidence `
    -ServiceUrl 'https://s3.example.com' `
    -Credential $s3Credential `
    -ForcePathStyle

$connectionString = Read-Host 'Azure Storage connection string' -AsSecureString
$blob = Connect-TransferettoAzureBlob `
    -ConnectionString $connectionString `
    -ContainerName evidence
```

Custom remote endpoints must use HTTPS. Loopback HTTP is accepted for local emulators. Transferetto performs data-plane operations only; it does not create buckets, create containers, assign roles, or administer storage accounts.

Metadata names use a portable subset shared by S3 and Azure Blob: ASCII letters, digits, and underscores, starting with a letter or underscore. This prevents provider-specific headers from breaking a cross-provider copy.

## .NET packages

| Package | Purpose |
| --- | --- |
| `Transferetto.Core` | Provider-neutral endpoints, streaming engine, filesystem endpoint, progress, integrity receipts, and metadata rules |
| `Transferetto.S3` | Amazon S3 and S3-compatible data-plane provider |
| `Transferetto.AzureBlob` | Azure Blob data-plane provider |
| `Transferetto` | FTP, FTPS, SFTP, SCP, SSH, and existing synchronization APIs |

The PowerShell module combines these assemblies behind one Transferetto command surface. .NET applications can reference only the packages they need.

## How this fits with other Evotec projects

Transferetto owns transport. DbaClientX owns database and provider access. FabricClientX owns Microsoft Fabric and Power BI workflows. OfficeIMO owns document and tabular formats.

That boundary keeps composition simple:

1. OfficeIMO or another producer writes an artifact.
2. Transferetto uploads it to S3 or Azure Blob and returns a transfer receipt.
3. FabricClientX ingests or refreshes data when the destination is Microsoft Fabric or Power BI.

OfficeIMO does not need AWS or Azure SDK dependencies. A file or stream is already the reusable handoff between the projects.

## Existing protocols

The existing examples cover FTP, FTPS, SFTP, SCP, SSH shells and tunnels, host-key and certificate policies, progress, streaming, and directory synchronization. See the [`Examples`](Examples) directory and the [protocol capability audit](Docs/ProtocolGapAudit.md).

## License

Transferetto is licensed under the [MIT License](LICENSE).
