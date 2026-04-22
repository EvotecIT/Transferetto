---
title: "Download files with FTP"
description: "Use Transferetto to list and download remote FTP files from PowerShell."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a job needs to pull files from a remote FTP folder and decide which ones to download based on the current listing.

It is adapted from the source example at `Examples/Example11-ReceiveFiles.ps1`.

## When to use this pattern

- You need to review a remote listing before downloading.
- Only some of the remote files should be downloaded.
- The local destination should stay under script control.

## Example

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential)
$remoteFiles = Get-FTPList -Client $ftpClient -Path '/exports' |
    Where-Object Type -eq File |
    Sort-Object Modified -Descending |
    Select-Object -First 2

Receive-FTPFile -Client $ftpClient `
    -RemoteFile $remoteFiles `
    -LocalPath "$PSScriptRoot\Download" `
    -LocalExists Overwrite

Disconnect-FTP -Client $ftpClient
```

## What this demonstrates

- listing a remote FTP folder before downloading
- filtering the selection with normal PowerShell pipeline logic
- downloading the resulting files into a controlled local directory

## Source

- [Example11-ReceiveFiles.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example11-ReceiveFiles.ps1)
