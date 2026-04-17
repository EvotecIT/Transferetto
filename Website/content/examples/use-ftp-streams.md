---
title: "Stream file content over FTP"
description: "Use Transferetto to write and read remote FTP content through managed streams."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a script should work with remote content incrementally instead of forcing a whole-file upload or download step first.

It is adapted from the source example at `Examples/Example22-FTPStream.ps1`.

## When to use this pattern

- You want to write or read content in chunks.
- The remote file may be inspected before downloading the whole thing.
- A streaming workflow is more natural than a file-based workflow.

## Example

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential)

$writeStream = Open-FTPStream -Client $ftpClient -RemotePath '/incoming/ftp-stream-demo.txt' -Mode Write
Write-FTPStream -StreamSession $writeStream -Text "first line`n"
Write-FTPStream -StreamSession $writeStream -Text "second line`n"
Sync-FTPStream -StreamSession $writeStream
Close-FTPStream -StreamSession $writeStream

$readStream = Open-FTPStream -Client $ftpClient -RemotePath '/incoming/ftp-stream-demo.txt' -Mode Read
$chunk = Read-FTPStream -StreamSession $readStream -Count 64 -AsText
$chunk

Close-FTPStream -StreamSession $readStream
Disconnect-FTP -Client $ftpClient
```

## What this demonstrates

- opening a managed FTP write stream
- syncing the remote stream explicitly before closing it
- opening a second stream to read remote content back as text

## Source

- [Example22-FTPStream.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example22-FTPStream.ps1)
