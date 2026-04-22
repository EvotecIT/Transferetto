---
title: "Transferetto capabilities"
description: "What Transferetto covers today across FTP, FTPS, SFTP, SCP, and SSH."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto is no longer just a collection of helper scripts. It is a reusable transport library with a PowerShell cmdlet layer on top, which means the protocol behavior is modeled once and then surfaced consistently.

## Protocol coverage

### FTP and FTPS

- connect with credentials, profiles, encryption settings, and proxy options
- upload and download files and directories
- inspect remote items, size, modified time, and working directory
- create directories, move content, and work with transfer status objects
- stream remote file content for chunked reads and writes

### SFTP

- connect with credentials or private keys
- upload and download files and directories
- inspect paths, permissions, timestamps, symlinks, and working directory
- create, move, and remove remote items and directories
- read and write remote content directly or through managed streams

### SCP

- connect with the same SSH authentication and host-key model used by the SSH lane
- send and receive files or whole directories when a server exposes SCP instead of SFTP

### SSH

- run one-shot commands and return structured results
- open interactive shell sessions with prompt-aware command execution
- capture rolling shell transcripts for troubleshooting or deployment review
- open and close local or remote tunnels for database, web, or admin access

## Design shape

- `Transferetto` contains the reusable C# transport logic
- `Transferetto.PowerShell` contains thin binary cmdlets over that core
- website examples stay curated and task-focused rather than mirroring every source example verbatim

## Good fit

- scheduled jobs that exchange files with partner or application endpoints
- deployment flows that need both file transfer and SSH management
- operational scripts that benefit from typed session objects and transfer results
- environments where SSH host-key validation and repeatable automation matter

## Related pages

- [Installation](../install/)
- [Connection and trust](../connection-and-trust/)
- [FTP and FTPS workflows](../ftp-ftps-workflows/)
- [SFTP and SCP workflows](../sftp-and-scp-workflows/)
- [SSH management guide](../ssh-management/)
- [Streams and session patterns](../streams-and-sessions/)
- [Examples](/projects/transferetto/examples/)
