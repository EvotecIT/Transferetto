---
title: "Download files with SFTP"
description: "Use Transferetto to download selected files from a remote SFTP folder."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a process needs to collect exported files from a partner or internal SFTP endpoint.

It is adapted from the source example at `Examples/Example12-Receive-SFTPFile.ps1`.

## When to use this pattern

- You need to download each file from a remote folder.
- The local landing folder is controlled by the script.
- You want a status object for each transfer.

## Example

```powershell
Import-Module Transferetto

$credential = Get-Credential
$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential $credential
$localFolder = Join-Path $PSScriptRoot 'Output\SftpDownload'

New-Item -ItemType Directory -Force -Path $localFolder | Out-Null

$remoteFiles = Get-SFTPList -SftpClient $sftpClient -Path '/pub/example' |
    Where-Object { -not $_.IsDirectory }

$output = foreach ($remoteFile in $remoteFiles) {
    $localPath = Join-Path $localFolder $remoteFile.Name
    Receive-SFTPFile -SftpClient $sftpClient -RemotePath $remoteFile.FullName -LocalPath $localPath
}

$output | Format-Table
Disconnect-SFTP -SftpClient $sftpClient
```

## What this demonstrates

- filtering remote folder entries to files
- downloading into a deterministic local folder
- returning transfer status objects for review

## Source

- [Example12-Receive-SFTPFile.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example12-Receive-SFTPFile.ps1)
