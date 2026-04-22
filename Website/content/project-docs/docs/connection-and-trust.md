---
title: "Connection and trust"
description: "How to think about credentials, private keys, host-key policy, and proxies in Transferetto."
layout: docs
meta.generated_by: "powerforge.project-docs-sync"
meta.project_base_slug: "transferetto"
meta.project_name: "Transferetto"
meta.project_section: "docs"
meta.project_hub_path: "/projects/transferetto/"
meta.project_link_docs: "/projects/transferetto/docs/"
---

Transferetto is most useful when the trust model is explicit. The cmdlets support multiple connection styles, but the best choice depends on the protocol and the environment you are automating.

## Credentials and keys

- Use `Get-Credential` when a PowerShell workflow should avoid embedding secrets in source.
- Use username and password parameters only for controlled local testing or generated configuration.
- Prefer SSH private keys for SFTP, SCP, and SSH when the target environment supports them.
- Keep proxy credentials separate from target credentials so the script stays clear about which hop each secret belongs to.

## SSH and SCP trust models

Transferetto exposes several host-key trust options through `Connect-SSH` and `Connect-SCP`:

- `-ExpectedHostKeyFingerprint` for explicit fingerprint pinning
- `-HostKeyPolicy KnownHosts` with `-KnownHostsPath` for strict pre-seeded validation
- `-HostKeyPolicy TrustOnFirstUse` for environments where you want first connection persistence but still avoid silent drift
- `-AcceptAnyHostKey` only for disposable test systems

For production servers, prefer a pinned fingerprint or a managed known-hosts file. That makes host identity reviewable and avoids surprise trust changes later.

## FTP and FTPS trust considerations

- Use FTPS when the remote side supports it and the workflow needs encrypted transport.
- Review certificate behavior explicitly when connecting to internal or lab systems.
- Proxy and profile options are useful for older endpoints, but they should still be documented in the script so the connection path is visible.

## Proxies, bastions, and tunnels

- FTP and FTPS support proxy-aware connection options.
- SSH and SCP support proxy configuration directly on connect.
- When a service is only reachable from the server itself, use an SSH tunnel instead of exposing the service publicly.

## Practical guidance

- Keep the connection object creation near the top of the script.
- Make the trust model readable in the script itself.
- Disconnect explicitly when the workflow is finished.
- Use examples as patterns, not as places to hard-code real production secrets.

## Related pages

- [SSH management guide](../ssh-management/)
- [SFTP and SCP workflows](../sftp-and-scp-workflows/)
- [Enforce SSH host-key policy example](/projects/transferetto/examples/enforce-ssh-host-key-policy/)
