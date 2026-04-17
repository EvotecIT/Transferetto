---
title: "Capture an SSH shell transcript"
description: "Use Transferetto to retain and export a transcript from an interactive SSH shell."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when you want to keep a readable record of what happened during an interactive shell workflow, especially for deployment or server troubleshooting.

It is adapted from the source example at `Examples/Example26-SSHShellTranscript.ps1`.

## When to use this pattern

- An automation runs several shell commands in sequence.
- You want to review the shell dialogue after the fact.
- The workflow should persist a transcript to disk for logs or investigation.

## Example

```powershell
Import-Module Transferetto

$sshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)
$shell = New-SSHShell -SshClient $sshClient -PromptPattern '(?m)^[^@\r\n]+@[^:\r\n]+:.*[$#]\s?$'

Invoke-SSHShellCommand -ShellSession $shell -Command 'cd /var/www/html && ls -la'
Invoke-SSHShellCommand -ShellSession $shell -Command 'sudo systemctl status nginx --no-pager'

Get-SSHShellTranscript -ShellSession $shell -AsText
Export-SSHShellTranscript -ShellSession $shell -Path "$PSScriptRoot\Logs\ssh-session.log"

Close-SSHShell -ShellSession $shell
Disconnect-SSH -SshClient $sshClient
```

## What this demonstrates

- running multiple commands through one shell session
- reading the accumulated transcript in text form
- exporting the transcript for later review

## Source

- [Example26-SSHShellTranscript.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example26-SSHShellTranscript.ps1)
