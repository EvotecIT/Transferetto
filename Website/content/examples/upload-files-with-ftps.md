---
title: "Upload files with FTPS"
description: "Use Transferetto to upload one or more files to an FTPS endpoint from PowerShell."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a PowerShell script needs to publish files to a classic FTP-family endpoint but still use encrypted transport.

It is adapted from the source example at `Examples/Example06-UploadFTPS.ps1`.

## When to use this pattern

- The target system exposes FTPS rather than SFTP.
- You want to upload one file, several files, or a whole selected set from disk.
- The remote location may need to be created as part of the transfer.

## Example

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' `
    -Credential (Get-Credential) `
    -EncryptionMode Explicit

$localFiles = Get-ChildItem -LiteralPath "$PSScriptRoot\Upload" -File

foreach ($file in $localFiles) {
    Send-FTPFile -Client $ftpClient `
        -LocalPath $file.FullName `
        -RemotePath "/incoming/$($file.Name)" `
        -RemoteExists Overwrite `
        -CreateRemoteDirectory
}

Disconnect-FTP -Client $ftpClient
```

## What this demonstrates

- connecting to an FTPS endpoint with explicit encryption
- selecting local files through normal PowerShell file handling
- uploading each file into a deterministic remote path

## Source

- [Example06-UploadFTPS.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example06-UploadFTPS.ps1)
