---
title: "FTP and FTPS workflows"
description: "How Transferetto approaches classic FTP and FTPS automation."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Use the FTP and FTPS lane when you need compatibility with classic endpoints, partner systems, appliances, or hosting environments that still expose FTP-family protocols.

## What the lane covers

- `Connect-FTP` with credentials, profiles, encryption options, and proxy settings
- file upload and download workflows
- directory transfer workflows
- remote metadata such as item info, file size, modified time, and working directory
- managed stream sessions for chunked reads and writes

## Good fit

- legacy or appliance endpoints that only expose FTP or FTPS
- shared-hosting workflows where you need to inspect `/public_html` style content
- scripts that need remote file metadata before deciding what to transfer
- workflows where streaming a file is more natural than staging a whole temp file locally

## Operating pattern

Start with a connection, confirm the working directory or target path, inspect any metadata you need, then choose the file, directory, or stream path that matches the task.

This usually means:

- `Connect-FTP`
- optional `Get-FTPWorkingDirectory`, `Set-FTPWorkingDirectory`, `Get-FTPItem`, or `Get-FTPModifiedTime`
- transfer or stream cmdlets
- `Disconnect-FTP`

## Why streams matter

Transferetto does not force every FTP workflow into whole-file upload or download operations. Managed stream cmdlets let a script write and read remote content incrementally, which is useful for generated text, inspection, and automation pipelines.

## Related pages

- [Capability guide](../capabilities/)
- [Streams and session patterns](../streams-and-sessions/)
- [Stream file content over FTP example](/projects/transferetto/examples/use-ftp-streams/)
