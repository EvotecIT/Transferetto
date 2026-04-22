# Transferetto protocol gap audit

Last updated: 2026-04-22

This document tracks the current Transferetto protocol surface after the migration to a reusable C# library and binary PowerShell cmdlets. It is intentionally implementation-focused: what exists today, what is missing for a fuller protocol surface, and what should be prioritized before building CLI or MCP layers.

## Current architecture

- `Transferetto` is the reusable C# library.
- `Transferetto.PowerShell` is a thin binary cmdlet layer over the library.
- Long-running transfer cmdlets now use an `AsyncPSCmdlet` base so PowerShell progress, cancellation, and pipeline writes remain safe while work executes asynchronously.
- The reusable library now exposes Task-based async transfer APIs for FTP, FXP, SFTP, and SCP operations, so CLI/MCP no longer need to start from a synchronous transfer-only surface.
- `Transferetto.Cli` exists as a stub, but no real CLI surface has been implemented yet.
- There is no MCP server or Codex skill surface yet.
- Module runtime is script-free; `Transferetto.psm1` loads the binary cmdlets.
- XML documentation and generated PowerShell help are enabled.
- Warnings-as-errors is enabled for the project build.

## Protocol coverage summary

| Area | Status | Notes |
| --- | --- | --- |
| FTP | Strong | File, directory, metadata, move/remove/rename, checksum, chmod, stream, tracing, proxy, auto-detect, transfer progress/cancellation, async transfer APIs, and option support exist. |
| FTPS | Strong | Exposed through `Connect-FTP` encryption/certificate options rather than a separate `Connect-FTPS` cmdlet. Certificate thumbprint pinning and known-certificate trust are available. |
| FXP | Present | Server-to-server FTP file and directory transfer exists through `Start-FXPFileTransfer` and `Start-FXPDirectoryTransfer`, with shared progress/cancellation support and a preflight check. |
| SFTP | Strong | File, directory, content, stream, metadata, permissions, timestamps, symlink, move/remove/rename, working-directory support, SSH-style trust/proxy/auth options, and transfer progress/cancellation hooks exist. |
| SCP | Basic but useful | File and directory send/receive with progress/cancellation exist. SCP intentionally has less filesystem-management surface than SFTP. |
| SSH commands | Strong | One-shot command execution exists with structured command result support, Task-based async execution, timeout/cancellation, and progressive stdout/stderr streaming callbacks. |
| SSH shell | Strong | Interactive shell, prompt detection, regex reads, idle reads, cancellation-aware waits, progressive output callbacks, follow-mode reads, expect-style automation, reusable Linux shell recipes, command invocation, transcripts, and resize support exist. |
| SSH tunnels | Present | Local and remote forwarded port sessions exist. |
| CLI | Stub only | No meaningful command-line interface yet. |
| MCP | Not started | Needs to sit on top of stable library and CLI-style operation contracts. |

## FTP and FTPS

### Implemented

- `Connect-FTP` / `Disconnect-FTP`
- credential and profile-based connection
- encryption mode selection for FTPS
- data connection type, certificate-revocation toggle, accept-any-certificate switch, keepalive, send-host, auto-connect
- proxy options
- runtime tuning for control/data timeouts, retry attempts, transfer chunk size, local buffer size, IP version, upload/download rate limits, data type selection, active/passive port tuning, FXP progress interval, and control-channel encoding
- listing and item inspection
- working directory get/set
- file upload and download
- directory upload and download
- file and directory move/remove/rename
- file and directory existence tests
- checksum and compare helpers
- chmod get/set
- modified-time get/set
- remote directory creation
- managed FTP streams: open, read, write, seek, sync, close
- shared transfer options, progress reporting, cancellation token checks, and Task-based async APIs for chunked FTP stream read/write
- tracing settings
- FXP file and directory transfer between two FTP sessions
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for file upload/download
- shared transfer options and progress/cancellation for directory upload/download
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for FXP file transfer
- shared transfer options and progress/cancellation for FXP directory transfer
- FXP preflight checks for connection state, source path, destination parent readiness, and advertised FXP-adjacent capabilities
- FTPS certificate thumbprint pinning with captured certificate metadata
- FTPS certificate policy selection with platform-chain, known-certificate, and trust-on-first-use modes

### Validated behavior

- Explicit FTPS connect, list, upload, and download were validated against a local WSL `vsftpd` harness with `require_ssl_reuse=YES`.
- Public explicit and implicit FTPS connections were validated against `test.rebex.net`.
- `UseGnuTls` works against the public Rebex FTPS server after native dependency preloading on Windows.
- A public `ftp.dlptest.com` explicit-FTPS attempt reproduced a real `522` session-reuse error during investigation, but that server was not stable enough to rely on as an automated regression target.

### Gaps

- No dedicated `Connect-FTPS` alias/cmdlet; FTPS is discoverable only through `Connect-FTP -EncryptionMode`.
- Certificate validation now supports expected thumbprints and a reusable known-certificate store, but there is no custom validation callback model.
- Not all FluentFTP configuration knobs are surfaced. The common runtime knobs are now available through `Connect-FTP`, but specialized parser/sanitizer policies, low-level socket internals, and custom retry/error-policy models remain future library work.
- Task-based async transfer APIs now exist for the major FTP and FXP transfer operations, and chunked FTP stream read/write now participate in the shared progress/cancellation model. The remaining gap is depth: the implementation still layers over the current synchronous transfer engine rather than using FluentFTP's async-native client surface end to end.
- No resumable transfer abstraction exposed as a first-class concept.
- No dedicated mirror/sync policy object. Directory upload/download takes sync-related options, but a reusable `TransferettoSyncOptions` model would be cleaner for CLI/MCP.
- FXP preflight is intentionally conservative; a server can still reject FXP later due to runtime policy, firewall, NAT, or passive/active mode restrictions.

### Recommended next slices

1. Add resumable transfer and sync policy models where FluentFTP support maps cleanly.
2. Add a custom validation callback model for advanced library consumers.
3. Add clearer runtime FXP error result mapping around server policy failures.
4. Deepen the async FTP/FXP implementation so it can use protocol-native async primitives where FluentFTP support maps cleanly.

## SFTP

### Implemented

- `Connect-SFTP` / `Disconnect-SFTP`
- credential, username/password, and private key connection
- listing and item inspection
- working directory get/set
- attributes, chmod, and timestamp operations
- file and directory existence tests
- symlink detection and symlink creation
- remote directory creation and removal
- file upload and download
- directory upload and download
- file and directory move
- file remove and rename
- text and byte content read/write helpers
- managed SFTP streams: open, read, write, seek, sync, close
- shared transfer options, progress reporting, cancellation token checks, and Task-based async APIs for chunked SFTP stream read/write

### Validated behavior

- Public SFTP listing and file download were validated against `test.rebex.net` with optional live PowerShell tests.
- Local SFTP downloads now write through an atomic temporary file and release the file handle before the final move on Windows.

### Gaps

- Task-based async transfer APIs now exist for SFTP upload and download operations, and chunked SFTP stream read/write now participate in the shared progress/cancellation model. SSH.NET async-native coverage is still not used consistently across the broader SFTP surface.
- Directory transfer options are simple. There is no reusable include/exclude filter model, dry-run mode, conflict policy object, or detailed recursive plan result.
- No rich ownership operations beyond chmod/timestamps. Owner/group IDs, where supported, are not surfaced as explicit set operations.
- No hard-link support. Symlink support is present.
- No explicit remote copy operation. SFTP protocol support depends on server extensions, so this should be capability-driven if added.

### Recommended next slices

1. Deepen the SFTP async implementation where SSH.NET exposes stable async primitives.
2. Add recursive transfer planning: include/exclude filters, dry-run, conflict policy, and summary result.
3. Add owner/group and richer attribute update support where SSH.NET/server support allows it.
4. Consider async/control wrappers for open, seek, flush, and close only if a real consumer scenario needs them.

## SCP

### Implemented

- `Connect-SCP` / `Disconnect-SCP`
- SSH-style credential/private-key/proxy/host-key policy connection options
- send and receive file
- send and receive directory
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for file and directory transfer

### Validated behavior

- Public SCP file download was validated against `test.rebex.net` with optional live PowerShell tests.

### Gaps

- SCP intentionally has no listing, metadata, chmod, or stream lane. That is normal; users should use SFTP for filesystem management.
- Task-based async transfer APIs now exist for SCP send and receive operations, but they still wrap the current synchronous implementation. Protocol-native async depth depends on what SSH.NET exposes reliably for SCP.
- Directory transfer result is coarser than SFTP/FTP recursive result handling.

### Recommended next slices

1. Normalize SCP transfer results with FTP/SFTP directory transfer result shapes.
2. Keep SCP small and document that SFTP is preferred for remote filesystem management.
3. Deepen SCP async behavior if SSH.NET exposes a stable protocol-native async surface.

## SSH command execution

### Implemented

- `Connect-SSH` / `Disconnect-SSH`
- username/password, credential, private key, private-key passphrase
- keyboard-interactive option
- host-key policy: expected fingerprint, known-hosts, trust-on-first-use, accept-any
- keepalive, timeout, retry
- proxy options
- one-shot command execution through `Send-SSHCommand`
- structured command result type with exit status, exit signal, timestamps, and cancellation state
- Task-based async command execution with shared command options
- per-command timeout and cancellation support
- progressive stdout/stderr streaming callbacks through reusable output chunk objects

### Validated behavior

- Public SSH command execution was validated against `test.rebex.net` with an opt-in live test that runs `pwd`.

### Gaps

- One-shot command execution still does not expose environment variables, working directory, or PTY allocation as first-class options.
- No non-interactive sudo helper for one-shot SSH commands. Privileged flows currently live in the interactive shell recipe layer.
- No command batch result object that preserves per-command exit status when multiple commands are sent.

### Recommended next slices

1. Add working directory, PTY, environment, and stdin options for non-interactive commands where SSH.NET support maps cleanly.
2. Add a command batch result model with per-command exit status and captured output.
3. Decide whether sudo/deployment helpers belong in the library or should stay as higher-level recipes.

## SSH interactive shell

### Implemented

- `New-SSHShell` / `Close-SSHShell`
- read, write, regex read, idle read
- Task-based async shell reads and waits with cancellation-aware polling
- prompt pattern set/wait with reusable common-shell presets
- shell command invocation with prompt/sentinel handling
- follow-mode shell output helper for long-running log and deploy output
- ordered expect-style shell automation with reusable step/result models
- reusable Linux shell recipes for `sudo`, `tail -f`, and `journalctl -f`
- progressive shell output chunk callbacks for read, wait, follow, and shell-command invocation
- shell control keys
- stop current shell command
- clear shell buffer
- resize shell
- rolling transcript capture, export, clear

### Gaps

- Prompt presets now reduce the need for ad-hoc regexes, but there is still no prompt discovery helper for unknown or custom shells.
- No secret redaction in transcripts.
- No structured transcript export formats beyond current text/file behavior.
- No terminal profile presets beyond raw dimensions and terminal name.
- No higher-level deployment/service recipes yet beyond `sudo`, file follow, and journal follow.

### Recommended next slices

1. Add transcript redaction options.
2. Add higher-level deployment and service-management recipes on top of the current `sudo` and follow primitives.
3. Add prompt discovery to complement the new preset patterns for common shells.
4. Consider stdin/input stream support for long-running interactive programs if real scenarios require it.

## SSH tunnels

### Implemented

- local forwarded port sessions
- remote forwarded port sessions
- tunnel stop/dispose

### Gaps

- No dynamic SOCKS tunnel support.
- No tunnel health/status polling beyond session state.
- No auto-reconnect or keepalive supervision at the tunnel level.
- No conflict detection for local bound ports before starting.
- No helper to run a command while a tunnel is open and then tear it down automatically.

### Recommended next slices

1. Add dynamic forwarding if SSH.NET support is available and stable enough.
2. Add tunnel status/health result and local port preflight.
3. Add `Use-SSHTunnel`-style scoped helper for PowerShell.

## Cross-cutting gaps

### Error model

Current operations return some result objects, but exceptions and error records are still inconsistent across cmdlets. A fuller model should standardize:

- protocol
- operation
- source path
- destination path
- status
- elapsed time
- bytes transferred
- retry count
- remote error code if available
- exception details

### Progress and observability

PowerShell can now show progress for the major transfer cmdlets through a shared cmdlet-side progress bridge, and CLI/MCP should eventually be able to stream the same events. SFTP, FTP/FTPS, FXP, SCP, and the chunked FTP/SFTP stream read/write APIs now share the same transfer progress abstraction. SSH commands expose progressive stdout/stderr chunk callbacks, and SSH shell reads/waits/follow/expect operations expose progressive shell output chunk callbacks through the reusable library and PowerShell layer. The same model can now serve higher-level deployment/login recipes and eventually CLI/MCP output.

### Async and cancellation

The library now exposes Task-based async APIs for the main transfer operations, chunked FTP/SFTP stream read/write, non-interactive SSH command execution, and shell read/wait/follow/expect flows. PowerShell cmdlets use those methods together with cancellation via `StopProcessing` and pipeline-safe output/progress marshaling. The remaining gap is depth and consistency: higher-level shell recipes, stream control operations if they ever need async semantics, richer SSH command options, and protocol-native async implementations still need to be added where the underlying libraries support them.

### Options model

Each protocol currently has its own options style. Before CLI/MCP, introduce common option models:

- `TransferettoTransferOptions`
- `TransferettoDirectoryTransferOptions`
- `TransferettoProgressOptions`
- `TransferettoRetryOptions`
- `TransferettoTrustOptions`

### Testing

Current tests cover model behavior and build/import health. A local FTPS integration harness now exists through `Build/Start-LocalFtpsServer.ps1`, `Build/Stop-LocalFtpsServer.ps1`, and `Tests/Local-FTPS.Tests.ps1`, using WSL Ubuntu with `vsftpd` on demand. That harness currently validates explicit FTPS listing plus upload/download round-trips through the PowerShell module. Public read-only SSH-family validation also exists through `Tests/Live-SSH.Tests.ps1`, gated by `TRANSFERETTO_RUN_LIVE_SSH_TESTS=1`, and currently covers SFTP listing/download, SCP download, and SSH command execution against `test.rebex.net`. Broader protocol integration coverage is still missing. Add optional live tests gated by environment variables for:

- FTP
- FTPS
- FXP
- SSH shell
- SSH tunnel

## Suggested implementation order

1. Normalize remaining result shapes where they still differ, especially SCP directory results versus FTP/SFTP recursive results.
2. Add SFTP recursive transfer planning and filters.
3. Add higher-level deployment and service-management helpers on top of the current shell recipes and expect primitives.
4. Finish remaining trust/validation and runtime error polish.
5. Extend async depth into richer SSH command options, shell reads/waits, and protocol-native implementations where supported.
6. Build CLI on top of the stable library contracts.
7. Build MCP tools on top of CLI/library operations.
