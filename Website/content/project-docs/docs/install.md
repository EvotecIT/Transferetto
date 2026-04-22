---
title: "Install Transferetto"
description: "Install Transferetto from PowerShell Gallery and start with the curated docs and examples."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Install the module from PowerShell Gallery before using the curated examples.

## PowerShell Gallery

```powershell
Install-Module Transferetto -Scope CurrentUser
```

## After install

Import the module and inspect the available cmdlets:

```powershell
Import-Module Transferetto
Get-Command -Module Transferetto
```

From there you can choose the lane you need:

- FTP and FTPS for classic file transfer and stream workflows
- SFTP for secure file and directory automation
- SCP when a server exposes SCP instead of SFTP
- SSH for commands, interactive shells, transcripts, and tunnels

## Next steps

- Read the [project overview](../overview/)
- Start with [Transfer files with PowerShell](../transfer-files-with-powershell/) if you want a practical first task
- Review the [capability guide](../capabilities/)
- Browse the [curated examples](/projects/transferetto/examples/)
- Open the [project source](https://github.com/EvotecIT/Transferetto)
