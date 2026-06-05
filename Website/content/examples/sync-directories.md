---
title: "Synchronize FTP and SFTP directories"
description: "Plan, preview, update, and mirror directory trees with Transferetto sync cmdlets."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

Use synchronization when a folder transfer should compare both sides before acting. The sync cmdlets can preview planned work, update only missing or changed files, mirror-delete extra destination items, filter paths, and choose how files are compared.

## Example

```powershell
Import-Module Transferetto

$credential = Get-Credential
$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential $credential

Sync-SFTPDirectory -SftpClient $sftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/releases' `
    -Mode Mirror `
    -Include '*.zip', '*.json' `
    -Exclude 'archive/*' `
    -DryRun

Sync-SFTPDirectory -SftpClient $sftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/releases' `
    -Mode Mirror `
    -Include '*.zip', '*.json' `
    -Exclude 'archive/*' `
    -ShowProgress

Disconnect-SFTP -SftpClient $sftpClient
```

## Useful options

- `-Direction Upload` treats the local folder as source.
- `-Direction Download` treats the remote folder as source.
- `-Mode Update` leaves extra destination files alone.
- `-Mode Mirror` removes destination files and folders that are not in the source.
- `-Comparison Size`, `-Comparison LastWriteTime`, and `-Comparison SizeOrLastWriteTime` control change detection.
- `-DryRun` returns the planned actions without changing local or remote files.

## Source

- [Example40-SyncDirectories.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example40-SyncDirectories.ps1)
