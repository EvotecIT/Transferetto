---
title: "Run Linux commands with SSH"
description: "Use Transferetto to send one-shot Linux commands over SSH from PowerShell."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when PowerShell should ask a Linux host for status or run a short sequence of commands without opening a longer interactive session.

It is adapted from the source example at `Examples/Example08-ConnectSSH.ps1`.

## When to use this pattern

- A few commands should run in order over SSH.
- You do not need prompt-aware shell automation yet.
- The result can be returned as command output directly.

## Example

```powershell
Import-Module Transferetto

$sshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)

$command = {
    'cd /var/www/html'
    'ls -la'
    'cat /etc/os-release'
}

Send-SSHCommand -SshClient $sshClient -Command $command
Disconnect-SSH -SshClient $sshClient
```

## What this demonstrates

- opening an SSH session from PowerShell
- sending multiple Linux commands as one scripted block
- returning command output without managing an interactive shell

## Source

- [Example08-ConnectSSH.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example08-ConnectSSH.ps1)
