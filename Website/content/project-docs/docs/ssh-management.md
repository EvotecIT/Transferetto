---
title: "SSH management"
description: "Use Transferetto for SSH commands, interactive shells, transcripts, and tunnels."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto includes a full SSH management lane alongside its transfer protocols, so you can move from file delivery into real server operations without switching modules.

## What the SSH lane covers

- `Connect-SSH` with credentials, keys, host-key fingerprint pinning, known-hosts validation, TOFU, timeouts, retries, and proxy settings
- `Send-SSHCommand` for one-shot command execution
- `New-SSHShell`, `Read-SSHShell`, and `Invoke-SSHShellCommand` for interactive shell automation
- `Get-SSHShellTranscript` and related transcript helpers for reviewing what happened in a session
- `Start-SSHLocalTunnel` and `Start-SSHRemoteTunnel` for forwarding traffic to remote services
- `Connect-SCP` and related SCP cmdlets when a server exposes SCP workflows

## Why this matters

This makes Transferetto practical for webserver management and deployment flows:

- upload or download release assets
- run shell commands to inspect the host
- tail logs or follow deployment output through an interactive shell
- expose a remote database or admin port safely through a local tunnel

## Security defaults

The public examples avoid real endpoints and passwords. In real use, prefer:

- `Get-Credential` over hard-coded passwords
- private keys where possible
- explicit host-key fingerprints or a known-hosts path for production systems
- `AcceptAnyHostKey` only for disposable environments

## Related pages

- [Project overview](../overview/)
- [Capability guide](../capabilities/)
- [Connection and trust](../connection-and-trust/)
- [Streams and session patterns](../streams-and-sessions/)
- [SSH examples](/projects/transferetto/examples/)
