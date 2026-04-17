# Transferetto protocol gap audit

Last updated: 2026-04-17

This document tracks the current Transferetto protocol surface after the migration to a reusable C# library and binary PowerShell cmdlets. It is intentionally implementation-focused: what exists today, what is missing for a fuller protocol surface, and what should be prioritized before building CLI or MCP layers.

## Current architecture

- `Transferetto` is the reusable C# library.
- `Transferetto.PowerShell` is a thin binary cmdlet layer over the library.
- `Transferetto.Cli` exists as a stub, but no real CLI surface has been implemented yet.
- There is no MCP server or Codex skill surface yet.
- Module runtime is script-free; `Transferetto.psm1` loads the binary cmdlets.
- XML documentation and generated PowerShell help are enabled.
- Warnings-as-errors is enabled for the project build.

## Protocol coverage summary

| Area | Status | Notes |
| --- | --- | --- |
| FTP | Strong | File, directory, metadata, move/remove/rename, checksum, chmod, stream, tracing, proxy, auto-detect, transfer progress/cancellation, and option support exist. |
| FTPS | Strong | Exposed through `Connect-FTP` encryption/certificate options rather than a separate `Connect-FTPS` cmdlet. Certificate thumbprint pinning and known-certificate trust are available. |
| FXP | Present | Server-to-server FTP file and directory transfer exists through `Start-FXPFileTransfer` and `Start-FXPDirectoryTransfer`, with shared progress/cancellation support and a preflight check. |
| SFTP | Strong | File, directory, content, stream, metadata, permissions, timestamps, symlink, move/remove/rename, working-directory support, SSH-style trust/proxy/auth options, and transfer progress/cancellation hooks exist. |
| SCP | Basic but useful | File and directory send/receive with progress/cancellation exist. SCP intentionally has less filesystem-management surface than SFTP. |
| SSH commands | Strong | One-shot command execution exists with structured command result support. |
| SSH shell | Strong | Interactive shell, prompt detection, regex reads, idle reads, control keys, command invocation, transcripts, and resize support exist. |
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
- tracing settings
- FXP file and directory transfer between two FTP sessions
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for file upload/download
- shared transfer options and progress/cancellation for directory upload/download
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for FXP file transfer
- shared transfer options and progress/cancellation for FXP directory transfer
- FXP preflight checks for connection state, source path, destination parent readiness, and advertised FXP-adjacent capabilities
- FTPS certificate thumbprint pinning with captured certificate metadata
- FTPS certificate policy selection with platform-chain, known-certificate, and trust-on-first-use modes

### Gaps

- No dedicated `Connect-FTPS` alias/cmdlet; FTPS is discoverable only through `Connect-FTP -EncryptionMode`.
- Certificate validation now supports expected thumbprints and a reusable known-certificate store, but there is no custom validation callback model.
- Not all FluentFTP configuration knobs are surfaced. The common runtime knobs are now available through `Connect-FTP`, but specialized parser/sanitizer policies, low-level socket internals, and custom retry/error-policy models remain future library work.
- No async public library API yet. Current API is synchronous, which is acceptable for PowerShell but less ideal for future CLI/MCP concurrency.
- No resumable transfer abstraction exposed as a first-class concept.
- No dedicated mirror/sync policy object. Directory upload/download takes sync-related options, but a reusable `TransferettoSyncOptions` model would be cleaner for CLI/MCP.
- FXP preflight is intentionally conservative; a server can still reject FXP later due to runtime policy, firewall, NAT, or passive/active mode restrictions.

### Recommended next slices

1. Add resumable transfer and sync policy models where FluentFTP support maps cleanly.
2. Add a custom validation callback model for advanced library consumers.
3. Add clearer runtime FXP error result mapping around server policy failures.
4. Add async library methods where the underlying libraries support them.

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

### Gaps

- SFTP now reuses the SSH/SCP trust/auth option model, including host-key policy, known-hosts, expected fingerprints, keepalive, timeout, retry, proxy, keyboard-interactive, and private-key passphrase.
- SFTP file and directory upload/download now support shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata.
- No async public library API despite SSH.NET exposing async operations for many SFTP scenarios.
- Streams do not yet expose the shared progress/cancellation model.
- Directory transfer options are simple. There is no reusable include/exclude filter model, dry-run mode, conflict policy object, or detailed recursive plan result.
- No rich ownership operations beyond chmod/timestamps. Owner/group IDs, where supported, are not surfaced as explicit set operations.
- No hard-link support. Symlink support is present.
- No explicit remote copy operation. SFTP protocol support depends on server extensions, so this should be capability-driven if added.

### Recommended next slices

1. Extend shared progress/cancellation to SFTP streams.
2. Add async public library methods where SSH.NET supports them.
3. Add recursive transfer planning: include/exclude filters, dry-run, conflict policy, and summary result.
4. Add owner/group and richer attribute update support where SSH.NET/server support allows it.

## SCP

### Implemented

- `Connect-SCP` / `Disconnect-SCP`
- SSH-style credential/private-key/proxy/host-key policy connection options
- send and receive file
- send and receive directory
- shared transfer options, progress reporting, cancellation token checks, byte counts, and timing metadata for file and directory transfer

### Gaps

- SCP intentionally has no listing, metadata, chmod, or stream lane. That is normal; users should use SFTP for filesystem management.
- No async public API.
- Directory transfer result is coarser than SFTP/FTP recursive result handling.

### Recommended next slices

1. Normalize SCP transfer results with FTP/SFTP directory transfer result shapes.
2. Keep SCP small and document that SFTP is preferred for remote filesystem management.
3. Add async public API if SSH.NET exposes a stable SCP async surface.

## SSH command execution

### Implemented

- `Connect-SSH` / `Disconnect-SSH`
- username/password, credential, private key, private-key passphrase
- keyboard-interactive option
- host-key policy: expected fingerprint, known-hosts, trust-on-first-use, accept-any
- keepalive, timeout, retry
- proxy options
- one-shot command execution through `Send-SSHCommand`
- structured command result type

### Gaps

- One-shot command execution does not expose environment variables, working directory, PTY allocation, command timeout per command, or cancellation as first-class options.
- No streaming stdout/stderr event model for long-running non-shell commands.
- No separate stdout/stderr pipeline objects for progressive output.
- No sudo helper pattern. This may be better as documentation/recipes than core API, but it matters for server management.
- No command batch result object that preserves per-command exit status when multiple commands are sent.

### Recommended next slices

1. Add `Invoke-SSHCommand` or enhance `Send-SSHCommand` with timeout, working directory, PTY, environment, and cancellation.
2. Add long-running command streaming callback support.
3. Add a command batch result model.

## SSH interactive shell

### Implemented

- `New-SSHShell` / `Close-SSHShell`
- read, write, regex read, idle read
- prompt pattern set/wait
- shell command invocation with prompt/sentinel handling
- shell control keys
- stop current shell command
- clear shell buffer
- resize shell
- rolling transcript capture, export, clear

### Gaps

- Prompt detection is regex-based and caller-driven. There is no prompt discovery helper yet.
- No reusable expect-script model with ordered send/expect steps.
- No high-level `Follow-SSHShellOutput` helper for `tail -f`, `journalctl -f`, and deployment logs.
- No secret redaction in transcripts.
- No structured transcript export formats beyond current text/file behavior.
- No cancellation-token support for waits/reads.
- No terminal profile presets beyond raw dimensions and terminal name.

### Recommended next slices

1. Add expect-style automation: ordered send/expect steps, timeout per step, and structured result.
2. Add transcript redaction options.
3. Add follow-mode helpers for logs and long-running deploy commands.
4. Add cancellation support for reads, waits, and command invocation.

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

PowerShell should be able to show progress for long transfers, and CLI/MCP should be able to stream progress events. SFTP, FTP/FTPS, FXP, and SCP transfers now use a shared progress abstraction. The same model still needs to be applied to streams and eventually CLI/MCP output.

### Async and cancellation

The current public API is synchronous. That is acceptable for simple PowerShell cmdlets, but future CLI and MCP tools will need async/cancellation for safe long-running operations.

### Options model

Each protocol currently has its own options style. Before CLI/MCP, introduce common option models:

- `TransferettoTransferOptions`
- `TransferettoDirectoryTransferOptions`
- `TransferettoProgressOptions`
- `TransferettoRetryOptions`
- `TransferettoTrustOptions`

### Testing

Current tests cover model behavior and build/import health, but there is no dedicated integration test harness. Add optional live tests gated by environment variables for:

- FTP
- FTPS
- FXP
- SFTP
- SCP
- SSH command
- SSH shell
- SSH tunnel

## Suggested implementation order

1. Extend shared transfer options, progress, cancellation, and result models across FTP, SCP, directories, and streams.
2. Add SFTP recursive transfer planning and filters.
3. Add SSH command streaming and expect-style shell automation.
4. Finish remaining trust/validation and runtime error polish.
5. Add async library methods where supported.
6. Build CLI on top of the stable library contracts.
7. Build MCP tools on top of CLI/library operations.

## Current worktree note

At the time of this audit, the uncommitted worktree includes website documentation, examples, shared transfer progress models, SFTP trust/progress work, and FTP/FXP/SCP progress/cancellation work.
