---
title: "Run an interactive SSH command"
description: "Use Transferetto to open a prompt-aware SSH shell and return structured command output."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a server-management step needs more than a one-shot command and should work through an interactive shell with prompt detection.

It is adapted from the source example at `Examples/Example24-SSHShellPromptAndCommand.ps1`.

## When to use this pattern

- You need a reusable interactive shell session.
- The target host uses a predictable prompt pattern.
- You want structured output, including the command result object.

## Example

```powershell
Import-Module Transferetto

$sshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)
$shell = New-SSHShell -SshClient $sshClient -PromptPattern '(?m)^[^@\r\n]+@[^:\r\n]+:[^\r\n]*[$#]\s?$'

Wait-SSHShellPrompt -ShellSession $shell -TimeoutSeconds 5 | Out-Null

$result = Invoke-SSHShellCommand -ShellSession $shell -Command 'pwd && whoami'
$result | Format-List

Close-SSHShell -ShellSession $shell
Disconnect-SSH -SshClient $sshClient
```

## What this demonstrates

- opening a prompt-aware interactive shell
- waiting until the prompt is ready before sending work
- running a shell command and returning a structured result

## Source

- [Example24-SSHShellPromptAndCommand.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example24-SSHShellPromptAndCommand.ps1)
