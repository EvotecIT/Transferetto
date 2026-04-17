---
title: "Manage Linux servers with PowerShell"
description: "Use Transferetto to run commands, open shells, capture transcripts, and expose services over SSH."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto is not only for file movement. Its SSH lane lets a PowerShell script inspect and operate a Linux host directly, which is useful for deployment, log review, service checks, and light operational automation.

## Typical management tasks

- run a few one-shot commands over SSH
- open an interactive shell and wait for prompts explicitly
- capture a transcript for later review
- open a local tunnel to reach a private service

## Send commands over SSH

```powershell
Import-Module Transferetto

$sshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)

$command = {
    'cd /var/www/html'
    'ls -la'
    'systemctl status nginx --no-pager'
}

Send-SSHCommand -SshClient $sshClient -Command $command
Disconnect-SSH -SshClient $sshClient
```

## When to use the shell lane instead

Use `New-SSHShell` and `Invoke-SSHShellCommand` when prompt awareness matters, the command sequence is interactive, or you want to keep a transcript of what happened during a longer session.

## Related pages

- [SSH management guide](../ssh-management/)
- [Connection and trust](../connection-and-trust/)
- [Run Linux commands with SSH example](/projects/transferetto/examples/run-linux-commands-with-ssh/)
- [Capture an SSH shell transcript example](/projects/transferetto/examples/capture-ssh-shell-transcript/)
