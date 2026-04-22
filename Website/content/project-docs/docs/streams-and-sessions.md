---
title: "Streams and session patterns"
description: "How Transferetto models reusable sessions, streams, and transcripts."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto is easier to compose than script-only modules because it uses typed session objects. You connect once, keep the returned session, and then pass that session into the cmdlets that work on the same endpoint or shell.

## Session types

- FTP session objects for FTP and FTPS operations
- SFTP session objects for secure file-management workflows
- SCP session objects for secure copy workflows
- SSH session objects for commands and tunnel creation
- SSH shell session objects for prompt-aware interactive work

## Why this matters

The script becomes easier to reason about:

- connection setup is separate from the transfer or shell steps
- trust and proxy decisions stay attached to the session that was created
- later commands are explicit about which remote endpoint they operate on

## Streams

Transferetto exposes managed stream sessions for FTP and SFTP. These are useful when:

- content is generated incrementally
- only part of a remote file needs to be read
- a script should avoid temporary staging for small content operations

The common pattern is:

- open a stream
- read or write one or more chunks
- sync or close the stream explicitly

## SSH transcripts

Interactive shell sessions can keep a rolling transcript. That is useful for:

- deployment reviews
- troubleshooting session behavior
- keeping a readable record of what an automation actually sent and received

## Related pages

- [SSH management guide](../ssh-management/)
- [Run an interactive SSH command example](/projects/transferetto/examples/run-ssh-shell-command/)
- [Capture an SSH shell transcript example](/projects/transferetto/examples/capture-ssh-shell-transcript/)
- [Stream file content over FTP example](/projects/transferetto/examples/use-ftp-streams/)
