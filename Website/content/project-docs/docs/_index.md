---
title: "Transferetto Docs"
description: "Curated documentation for Transferetto's binary PowerShell cmdlets and reusable transport library."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto provides reusable FTP, FTPS, SFTP, SCP, and SSH automation for PowerShell. The public module surface is a binary cmdlet layer over a C# transport library, so the same capability set can grow into CLI and MCP experiences later without rewriting the protocol logic.

## Start here

- [Installation](./install/)
- [Project overview](./overview/)
- [Transfer files with PowerShell](./transfer-files-with-powershell/)
- [Transfer folders with PowerShell](./transfer-folders-with-powershell/)
- [Inspect remote files with PowerShell](./inspect-remote-files-with-powershell/)
- [Manage Linux servers with PowerShell](./manage-linux-with-powershell/)
- [Capability guide](./capabilities/)
- [Connection and trust](./connection-and-trust/)
- [FTP and FTPS workflows](./ftp-ftps-workflows/)
- [SFTP and SCP workflows](./sftp-and-scp-workflows/)
- [SSH management guide](./ssh-management/)
- [Streams and session patterns](./streams-and-sessions/)
- [Curated examples](/projects/transferetto/examples/)
- [Back to project overview](/projects/transferetto/)

## Typical use

- connect to FTP, FTPS, SFTP, SCP, or SSH endpoints with credentials, keys, proxies, and host-key policy
- upload, download, list, and inspect files and directories as part of scheduled automation
- stream file content over FTP or SFTP instead of forcing whole-file workflows
- manage Linux servers through SSH commands, interactive shells, transcripts, and local tunnels

## Recommended reading order

- Start with [Project overview](./overview/) if you want the high-level shape.
- Jump straight to the task guides if you are trying to transfer files or operate a server from PowerShell today.
- Read [Connection and trust](./connection-and-trust/) before touching production hosts.
- Use the protocol guides to choose the right lane for FTP/FTPS, SFTP/SCP, or SSH.
- Finish with the [examples](/projects/transferetto/examples/) for task-focused snippets.
