---
title: "Transfer files with PowerShell"
description: "Use Transferetto to upload or download files over FTP, FTPS, SFTP, or SCP from PowerShell."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto gives PowerShell a consistent way to move files across classic FTP, secure file transfer, and SSH-based copy lanes. The main decision is not “can PowerShell do this,” but “which protocol matches the server I am talking to.”

## Choose the lane

- Use FTP or FTPS when the remote endpoint is a classic hosting or partner system.
- Use SFTP when you want secure file transfer plus richer remote filesystem operations.
- Use SCP when the server exposes simple SSH copy semantics and you do not need the broader SFTP surface.

## Common pattern

Most file-transfer scripts follow the same shape:

1. Connect and keep the returned session object.
2. Optionally list or inspect the remote path first.
3. Upload or download one or more files.
4. Disconnect explicitly.

## Upload a file with SFTP

```powershell
Import-Module Transferetto

$sftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)
Send-SFTPFile -SftpClient $sftpClient -LocalPath "$PSScriptRoot\Build\artifact.zip" -RemotePath '/incoming/artifact.zip' -AllowOverride
Disconnect-SFTP -SftpClient $sftpClient
```

## Download files with FTP

```powershell
Import-Module Transferetto

$ftpClient = Connect-FTP -Server 'ftp.example.com' -Credential (Get-Credential)
$files = Get-FTPList -Client $ftpClient -Path '/exports' | Where-Object Type -eq File
Receive-FTPFile -Client $ftpClient -RemoteFile $files -LocalPath "$PSScriptRoot\Download" -LocalExists Overwrite
Disconnect-FTP -Client $ftpClient
```

## Good habits

- Keep credentials outside the script source.
- Decide how overwrite behavior should work before running the transfer.
- Return or review transfer result objects when the job matters operationally.

## Related pages

- [Transfer folders with PowerShell](../transfer-folders-with-powershell/)
- [Connection and trust](../connection-and-trust/)
- [Upload files with SFTP example](/projects/transferetto/examples/upload-sftp-files/)
- [Upload files with FTPS example](/projects/transferetto/examples/upload-files-with-ftps/)
- [Download files with FTP example](/projects/transferetto/examples/download-files-with-ftp/)
