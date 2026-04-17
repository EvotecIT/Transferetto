---
title: "Inspect FTP files and folders"
description: "Use Transferetto to review FTP metadata, working directories, file sizes, and timestamps."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a script should inspect a remote FTP location before changing it, downloading from it, or publishing into it.

It is adapted from the source example at `Examples/Example29-FTPMetadataAndWorkdir.ps1`.

## When to use this pattern

- You need to confirm the current working directory.
- Metadata such as size or modified time should influence the next step.
- The script may need to create a directory before uploading into it.

## Example

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential)

Get-FTPWorkingDirectory -Client $ftpClient
Set-FTPWorkingDirectory -Client $ftpClient -Path '/public_html'
Get-FTPItem -Client $ftpClient -RemotePath '/public_html/index.html'
Get-FTPFileSize -Client $ftpClient -RemotePath '/public_html/index.html'
Get-FTPModifiedTime -Client $ftpClient -RemotePath '/public_html/index.html'

New-FTPDirectory -Client $ftpClient -RemotePath '/public_html/releases' -Force
Disconnect-FTP -Client $ftpClient
```

## What this demonstrates

- confirming and changing the remote working directory
- reading remote metadata before transfer or deployment steps
- creating a directory in preparation for later uploads

## Source

- [Example29-FTPMetadataAndWorkdir.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example29-FTPMetadataAndWorkdir.ps1)
