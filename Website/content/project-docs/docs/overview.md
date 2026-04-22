---
title: "Transferetto overview"
description: "Transferetto provides reusable FTP, FTPS, SFTP, SCP, and SSH automation for PowerShell."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Use Transferetto when a PowerShell process needs both transport and remote-management capability. The project now exposes a reusable C# transport library with thin binary PowerShell cmdlets on top, which keeps the protocol logic consistent while making future CLI and MCP surfaces realistic.

## Good fit

- uploading generated files to FTP, FTPS, SFTP, or SCP endpoints
- downloading partner or application exports with typed status results
- managing Linux hosts through SSH commands, interactive shells, transcripts, and tunnels
- combining transfer, inspection, permissions, and shell automation inside a single PowerShell workflow

## What Transferetto includes

- FTP and FTPS cmdlets for connection, listing, metadata, directories, and stream-based file access
- SFTP cmdlets for file transfer, directory transfer, permissions, timestamps, symlinks, and streams
- SCP cmdlets for file and directory copy workflows over SSH
- SSH cmdlets for one-shot commands, prompt-aware shell sessions, transcripts, and tunnels

## Architecture

- `Transferetto` contains the reusable .NET protocol layer
- `Transferetto.PowerShell` provides the binary cmdlet surface
- website examples stay intentionally curated so the public docs show common patterns without dumping every repository sample into the site

## Read by workflow

- start with [Transfer files with PowerShell](../transfer-files-with-powershell/) if the job is file movement
- start with [Transfer folders with PowerShell](../transfer-folders-with-powershell/) if the job is recursive copy
- start with [Inspect remote files with PowerShell](../inspect-remote-files-with-powershell/) if the script should list and verify before acting
- start with [Manage Linux servers with PowerShell](../manage-linux-with-powershell/) if the host itself is part of the automation
- choose [FTP and FTPS workflows](../ftp-ftps-workflows/) when compatibility with classic endpoints matters
- choose [SFTP and SCP workflows](../sftp-and-scp-workflows/) when secure file delivery is the main job
- choose [SSH management guide](../ssh-management/) when the script needs to operate the remote host itself
- read [Streams and session patterns](../streams-and-sessions/) when you want the reusable object model behind the cmdlets

## Related project pages

- [Installation](../install/)
- [Transfer files with PowerShell](../transfer-files-with-powershell/)
- [Transfer folders with PowerShell](../transfer-folders-with-powershell/)
- [Inspect remote files with PowerShell](../inspect-remote-files-with-powershell/)
- [Manage Linux servers with PowerShell](../manage-linux-with-powershell/)
- [Capability guide](../capabilities/)
- [Connection and trust](../connection-and-trust/)
- [FTP and FTPS workflows](../ftp-ftps-workflows/)
- [SFTP and SCP workflows](../sftp-and-scp-workflows/)
- [SSH management guide](../ssh-management/)
- [Streams and session patterns](../streams-and-sessions/)
- [Examples](/projects/transferetto/examples/)
- [Project overview](/projects/transferetto/)
