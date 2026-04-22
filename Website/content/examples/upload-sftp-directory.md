---
title: "Sync a folder with SFTP"
description: "Use Transferetto to upload or download an entire directory tree over SFTP."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a job needs to publish a prepared folder or pull down a full export instead of handling files one by one.

It is adapted from the source example at `Examples/Example23-SFTPDirectoryTransfer.ps1`.

## When to use this pattern

- You need to move a whole directory tree.
- The local and remote root folders are known ahead of time.
- You want Transferetto to return transfer result objects for each item.

## Example

```powershell
Import-Module Transferetto

$credential = Get-Credential
$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential $credential

Send-SFTPDirectory -SftpClient $sftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/releases' `
    -AllowOverride

Receive-SFTPDirectory -SftpClient $sftpClient `
    -RemotePath '/outgoing/reports' `
    -LocalPath "$PSScriptRoot\Download\Reports" `
    -AllowOverride

Disconnect-SFTP -SftpClient $sftpClient
```

## What this demonstrates

- sending a whole local folder into a remote SFTP location
- receiving a remote directory tree into a deterministic local path
- reusing the same SFTP session for both upload and download work

## Source

- [Example23-SFTPDirectoryTransfer.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example23-SFTPDirectoryTransfer.ps1)
