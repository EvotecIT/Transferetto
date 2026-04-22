---
title: "Open a local SSH tunnel"
description: "Use Transferetto to forward a remote service to a local port through SSH."
layout: docs
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "examples"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_examples: "/projects/transferetto/examples/"
---

This pattern is useful when a remote database or private service should be reachable locally for the duration of an SSH session.

It is adapted from the source example at `Examples/Example17-SSHLocalTunnel.ps1`.

## When to use this pattern

- A remote service is bound to localhost on the server.
- You want to expose it locally without changing the service itself.
- The tunnel should exist only while your script keeps the SSH session open.

## Example

```powershell
Import-Module Transferetto

$sshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)

$tunnel = Start-SSHLocalTunnel -SshClient $sshClient `
    -BoundHost '127.0.0.1' `
    -BoundPort 15432 `
    -RemoteHost '127.0.0.1' `
    -RemotePort 5432

$tunnel | Format-List

# Use 127.0.0.1:15432 locally while the tunnel remains open.
Read-Host 'Press Enter to stop the tunnel'

Stop-SSHTunnel -TunnelSession $tunnel
Disconnect-SSH -SshClient $sshClient
```

## What this demonstrates

- creating a local forwarded port for a remote service
- keeping the tunnel lifecycle explicit in the script
- closing the tunnel before disconnecting the SSH session

## Source

- [Example17-SSHLocalTunnel.ps1](https://github.com/EvotecIT/Transferetto/blob/master/Examples/Example17-SSHLocalTunnel.ps1)
