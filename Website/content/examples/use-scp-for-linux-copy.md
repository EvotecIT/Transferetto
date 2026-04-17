---
title: "Copy files with SCP"
description: "Use Transferetto to copy files and directories with SCP."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when the target system exposes SCP and you want a straightforward Linux-style copy workflow without the richer SFTP filesystem surface.

It is adapted from the source example at `Examples/Example02-BasicExampleSCP.ps1`.

## When to use this pattern

- The server exposes SCP.
- The job is mostly file or directory copy.
- You want the same SSH trust model used by the SSH management lane.

## Example

```powershell
Import-Module Transferetto

$scpClient = Connect-SCP -Server 'server.example.com' -Credential (Get-Credential)

Receive-SCPFile -ScpClient $scpClient `
    -RemotePath '/var/www/releases/current.zip' `
    -LocalPath "$PSScriptRoot\Downloads\current.zip"

Send-SCPDirectory -ScpClient $scpClient `
    -LocalPath "$PSScriptRoot\BuildOutput" `
    -RemotePath '/var/www/releases/build-output'

Disconnect-SCP -ScpClient $scpClient
```

## What this demonstrates

- opening an SCP session with the same SSH credential flow as other secure lanes
- downloading a single remote file
- uploading a full local directory tree

## Source

- [Example02-BasicExampleSCP.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example02-BasicExampleSCP.ps1)
