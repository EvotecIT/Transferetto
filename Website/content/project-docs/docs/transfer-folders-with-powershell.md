---
title: "Transfer folders with PowerShell"
description: "Use Transferetto to send or receive whole directory trees from PowerShell."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Folder transfer is usually where ad-hoc scripts get messy. Transferetto gives you explicit directory cmdlets so you do not have to loop over every item manually unless you want that level of control.

## Directory transfer lanes

- `Send-SFTPDirectory` and `Receive-SFTPDirectory` for secure recursive transfer
- `Send-FTPDirectory` and `Receive-FTPDirectory` for classic FTP and FTPS endpoints
- `Send-SCPDirectory` and `Receive-SCPDirectory` when the server exposes SCP

## When to prefer each one

- Prefer SFTP for secure recursive transfer with richer remote filesystem support.
- Use FTP or FTPS when the target system only exposes the FTP family.
- Use SCP when you need straightforward copy semantics and the server already supports SSH.

## Upload a folder with FTPS

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential) -EncryptionMode Explicit
Send-FTPDirectory -Client $ftpClient -LocalPath "$PSScriptRoot\Upload" -RemotePath '/releases' -FolderSyncMode Update
Disconnect-FTP -Client $ftpClient
```

## Receive a folder with SFTP

```powershell
Import-Module Transferetto

$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)
Receive-SFTPDirectory -SftpClient $sftpClient -RemotePath '/exports/nightly' -LocalPath "$PSScriptRoot\Download\Nightly" -AllowOverride
Disconnect-SFTP -SftpClient $sftpClient
```

## Practical guidance

- Use deterministic local and remote root paths.
- Decide whether existing files should be updated or preserved.
- Review the returned transfer results when the folder contents matter.

## Related pages

- [Transfer files with PowerShell](../transfer-files-with-powershell/)
- [SFTP and SCP workflows](../sftp-and-scp-workflows/)
- [Sync a folder with SFTP example](/projects/transferetto/examples/upload-sftp-directory/)
- [Manage SFTP folders example](/projects/transferetto/examples/manage-sftp-folders/)
