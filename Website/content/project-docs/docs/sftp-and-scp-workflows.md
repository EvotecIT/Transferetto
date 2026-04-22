---
title: "SFTP and SCP workflows"
description: "How Transferetto handles secure file movement with SFTP and SCP."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto treats SFTP as the richer secure file-management lane and SCP as the compatibility lane for systems that expose copy semantics without the broader SFTP surface.

## Use SFTP when you need

- directory listings and working-directory control
- recursive upload or download
- path inspection and existence tests
- permissions and timestamps
- symlink-aware file-management workflows
- content helpers or managed stream access

## Use SCP when you need

- simple file or directory copy over SSH
- compatibility with servers where SCP is available but SFTP is not
- the same host-key policy model as the SSH lane

## Practical distinction

If you need to inspect, manipulate, or reason about the remote filesystem, prefer SFTP. If you only need to copy files and directories and the server offers SCP, SCP can stay simpler.

## Session model

- `Connect-SFTP` returns a reusable SFTP session object for listings, transfers, metadata, permissions, and streams
- `Connect-SCP` returns a reusable SCP session object for copy operations over SSH

That means you can establish trust once and keep the rest of the workflow focused on the transfer task itself.

## Related pages

- [Connection and trust](../connection-and-trust/)
- [Sync a folder with SFTP example](/projects/transferetto/examples/upload-sftp-directory/)
- [Manage SFTP permissions example](/projects/transferetto/examples/manage-sftp-permissions/)
- [Copy files with SCP example](/projects/transferetto/examples/use-scp-for-linux-copy/)
