---
title: "Inspect remote files with PowerShell"
description: "Use Transferetto to list, inspect, and verify remote files and folders before acting on them."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

A lot of transfer automation should inspect first and act second. Transferetto includes listing and metadata cmdlets so a script can decide what to move before it starts writing or downloading content.

## Common inspection tasks

- list the contents of a remote folder
- filter to files or directories
- inspect timestamps, sizes, and effective paths
- check the current working directory or switch into a known one
- test whether a path exists before creating or overwriting it

## FTP inspection

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential)
Get-FTPWorkingDirectory -Client $ftpClient
Get-FTPList -Client $ftpClient -Path '/public_html'
Get-FTPItem -Client $ftpClient -RemotePath '/public_html/index.html'
Get-FTPFileSize -Client $ftpClient -RemotePath '/public_html/index.html'
Disconnect-FTP -Client $ftpClient
```

## SFTP inspection

```powershell
Import-Module Transferetto

$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)
Get-SFTPList -SftpClient $sftpClient -Path '/var/www/site'
Get-SFTPItem -SftpClient $sftpClient -Path '/var/www/site/index.html'
Get-SFTPChmod -SftpClient $sftpClient -Path '/var/www/site/index.html'
Disconnect-SFTP -SftpClient $sftpClient
```

## Why this matters

Inspection first makes scripts safer. It lets you filter down to the items you actually want, review metadata, and make overwrite or cleanup decisions from the data the server is returning right now.

## Related pages

- [Transfer files with PowerShell](../transfer-files-with-powershell/)
- [Manage SFTP permissions example](/projects/transferetto/examples/manage-sftp-permissions/)
- [Inspect FTP files and folders example](/projects/transferetto/examples/inspect-ftp-files-and-folders/)
