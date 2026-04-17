---
title: "Manage SFTP folders"
description: "Use Transferetto to create, inspect, test, and remove SFTP directories from PowerShell."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a script needs to prepare, verify, and clean up remote SFTP folders instead of only moving files.

It is adapted from the source example at `Examples/Example18-SFTPDirectoryManagement.ps1`.

## When to use this pattern

- The script should create a remote directory before transfer.
- You want to verify whether the folder exists.
- Working-directory changes or cleanup are part of the task.

## Example

```powershell
Import-Module Transferetto

$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)

New-SFTPDirectory -SftpClient $sftpClient -Path '/incoming/releases'
Test-SFTPDirectory -SftpClient $sftpClient -Path '/incoming/releases'
Get-SFTPItem -SftpClient $sftpClient -Path '/incoming'

Set-SFTPWorkingDirectory -SftpClient $sftpClient -Path '/incoming' -PassThru | Out-Null
Get-SFTPWorkingDirectory -SftpClient $sftpClient

Remove-SFTPDirectory -SftpClient $sftpClient -Path '/incoming/releases'
Disconnect-SFTP -SftpClient $sftpClient
```

## What this demonstrates

- creating and verifying a remote SFTP directory
- inspecting the parent folder contents
- changing the working directory before cleanup

## Source

- [Example18-SFTPDirectoryManagement.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example18-SFTPDirectoryManagement.ps1)
