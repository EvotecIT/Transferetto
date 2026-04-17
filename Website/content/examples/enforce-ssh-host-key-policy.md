---
title: "Enforce SSH host-key policy"
description: "Use Transferetto to pin host keys, validate known hosts, or use TOFU intentionally."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when you want SSH automation to be explicit about trust instead of silently accepting a server key.

It is adapted from the source example at `Examples/Example20-SSHHostKeyPolicy.ps1`.

## When to use this pattern

- You are connecting to production or partner systems.
- You need repeatable host-key validation behavior.
- You want a documented path for pinned fingerprints, known-hosts files, or TOFU.

## Example

```powershell
Import-Module Transferetto

$expectedFingerprint = 'SHA256:REPLACE_WITH_SERVER_FINGERPRINT'
$knownHostsPath = Join-Path $env:LOCALAPPDATA 'Transferetto\ssh-known-hosts.tsv'

$sshClient = Connect-SSH -Server 'server.example.com' `
    -Credential (Get-Credential) `
    -ExpectedHostKeyFingerprint $expectedFingerprint `
    -ConnectionTimeoutSeconds 15 `
    -KeepAliveIntervalSeconds 30 `
    -RetryAttempts 2

$sshClient.HostKeyInfo | Format-List
Disconnect-SSH -SshClient $sshClient
```

## Other supported trust models

- `-HostKeyPolicy TrustOnFirstUse -KnownHostsPath $knownHostsPath`
- `-HostKeyPolicy KnownHosts -KnownHostsPath $knownHostsPath`
- `-AcceptAnyHostKey` for disposable test environments only

## What this demonstrates

- connecting with an explicit expected host-key fingerprint
- inspecting the returned host-key metadata on the session
- keeping the trust decision visible in the script instead of implicit

## Source

- [Example20-SSHHostKeyPolicy.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example20-SSHHostKeyPolicy.ps1)
