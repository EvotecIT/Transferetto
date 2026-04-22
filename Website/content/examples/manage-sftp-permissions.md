---
title: "Manage SFTP permissions"
description: "Use Transferetto to inspect SFTP attributes, update chmod values, and change timestamps."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a deployment or maintenance script needs to inspect remote attributes and then update permissions or timestamps in a controlled way.

It is adapted from the source example at `Examples/Example27-SFTPAttributesAndPermissions.ps1`.

## When to use this pattern

- You need to confirm the current metadata on a remote file.
- The workflow should set a specific chmod value.
- A timestamp change should be part of the release or maintenance step.

## Example

```powershell
Import-Module Transferetto

$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)

Get-SFTPItem -SftpClient $sftpClient -Path '/var/www/site/index.html'
Get-SFTPChmod -SftpClient $sftpClient -Path '/var/www/site/index.html'

Set-SFTPChmod -SftpClient $sftpClient -Path '/var/www/site/index.html' -Permissions '644' -PassThru
Set-SFTPTimestamp -SftpClient $sftpClient -Path '/var/www/site/index.html' -LastWriteTime (Get-Date).AddMinutes(-5) -PassThru

Disconnect-SFTP -SftpClient $sftpClient
```

## What this demonstrates

- inspecting remote file attributes and effective permissions
- setting an octal-style chmod value through the SFTP lane
- updating the last-write timestamp on a remote file

## Source

- [Example27-SFTPAttributesAndPermissions.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example27-SFTPAttributesAndPermissions.ps1)
