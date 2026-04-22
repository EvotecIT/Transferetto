---
title: "Upload files with SFTP"
description: "Use Transferetto to upload local files into a remote SFTP folder."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a scheduled job needs to publish generated files to an SFTP endpoint.

It is adapted from the source example at `Examples/Example07-UploadSFTP.ps1`.

## When to use this pattern

- You have a folder of files that should be uploaded.
- The remote path is known ahead of time.
- Credentials should be prompted or retrieved securely at runtime.

## Example

```powershell
Import-Module Transferetto

$credential = Get-Credential
$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential $credential

Get-SFTPList -SftpClient $sftpClient -Path '/incoming' | Format-Table

Get-ChildItem -LiteralPath "$PSScriptRoot\Upload" -File | ForEach-Object {
    Send-SFTPFile -SftpClient $sftpClient -LocalPath $_.FullName -RemotePath "/incoming/$($_.Name)" -AllowOverride
}

Disconnect-SFTP -SftpClient $sftpClient
```

## What this demonstrates

- connecting with a PowerShell credential
- listing a remote SFTP folder
- uploading local files with explicit remote paths

## Source

- [Example07-UploadSFTP.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example07-UploadSFTP.ps1)
