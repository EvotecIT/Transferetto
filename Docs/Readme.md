---
Module Name: Transferetto
Module Guid: 7d61db15-9efe-41d1-a1c0-81d738975dec
Download Help Link: https://github.com/EvotecIT/Transferetto/blob/master/README.md
Help Version: 2.0.1
Locale: en-US
---
# Transferetto Module
## Description
Transferetto provides reusable .NET and PowerShell data transfer across filesystems, FTP, FTPS, SFTP, SCP, FXP, Amazon S3 and S3-compatible storage, Azure Blob Storage, and SSH operations.

## Transferetto Cmdlets
### [Clear-SSHShellBuffer](Clear-SSHShellBuffer.md)
Clears buffered unread output from an interactive SSH shell session.

Returns the buffered text by default so callers can inspect or discard it explicitly, or suppress output when they simply want a clean shell buffer before continuing an automation flow.

### [Clear-SSHShellTranscript](Clear-SSHShellTranscript.md)
Clears the in-memory transcript stored for an interactive SSH shell session.

Returns the removed transcript snapshot by default so callers can archive it before clearing, or suppress output when they simply want to reset transcript state before the next automation step.

### [Close-FTPStream](Close-FTPStream.md)
Closes an open FTP stream session.

Releases the low-level FTP stream created by Open-FTPStream so the remote file handle and associated transfer resources are closed cleanly.

### [Close-SFTPStream](Close-SFTPStream.md)
Closes an open SFTP stream session.

Releases the low-level SFTP stream created by Open-SFTPStream so the remote file handle and associated transport resources are closed cleanly.

### [Close-SSHShell](Close-SSHShell.md)
Closes an interactive SSH shell session.

Stops the shell stream created by New-SSHShell while leaving the parent SSH connection available for other commands or for creating a replacement shell later.

### [Close-TransferEndpoint](Close-TransferEndpoint.md)
Disposes a Transferetto endpoint and its owned provider client.

### [Compare-FTPFile](Compare-FTPFile.md)
Compares a local file with a remote FTP file.

Uses FluentFTP comparison strategies to determine whether a local file matches a remote file by size, hash, or server-supported auto-detection logic.

### [Connect-FTP](Connect-FTP.md)
Creates an FTP or FTPS session with runtime tuning, proxy support, and certificate trust controls.

Supports classic username/password and credential-based authentication, FluentFTP profiles, FTPS encryption modes, proxy settings, trust-on-first-use and known-certificate validation, plus transfer/runtime tuning that can be reused by later FTP and FTPS cmdlets.

### [Connect-SCP](Connect-SCP.md)
Creates an SCP session with SSH host-key validation, proxy support, and password or private-key authentication.

Uses the same SSH trust and authentication model as the SFTP and SSH cmdlets, making it easy to connect with credentials or private keys, enforce known-hosts or TOFU trust, and route SCP transfers through an SSH proxy when needed.

### [Connect-SFTP](Connect-SFTP.md)
Creates an SFTP session with SSH host-key trust, proxy support, and password or private-key authentication.

Supports clear-text credentials, PSCredential objects, private keys, keyboard-interactive auth, trust-on-first-use and known-hosts validation, connection retries, keepalive settings, and SSH proxy configuration that can be reused by later SFTP cmdlets.

### [Connect-SSH](Connect-SSH.md)
Creates a reusable SSH session for one-shot commands, interactive shells, and SSH tunnels.

Supports password, PSCredential, and private-key authentication together with keyboard-interactive auth, TOFU or known-hosts validation, retry and keepalive settings, and SSH proxy configuration for shell, tunnel, and command-based workflows.

### [Copy-TransferItem](Copy-TransferItem.md)
Streams an item between any two Transferetto endpoints.

The content is relayed without loading the full item into memory. The returned receipt contains a provider-independent SHA-256 digest.

### [Disconnect-FTP](Disconnect-FTP.md)
Disconnects an FTP or FTPS session.

Closes the reusable FTP session created by Connect-FTP so sockets and authentication state are released cleanly at the end of a script or pipeline.

### [Disconnect-SCP](Disconnect-SCP.md)
Disconnects an SCP session.

Closes the reusable SCP session created by Connect-SCP so the underlying SSH transport is released cleanly after copy operations complete.

### [Disconnect-SFTP](Disconnect-SFTP.md)
Disconnects an SFTP session.

Closes the reusable SFTP session created by Connect-SFTP so the underlying SSH transport is released cleanly when file operations are done.

### [Disconnect-SSH](Disconnect-SSH.md)
Disconnects an SSH session.

Closes the reusable SSH session created by Connect-SSH, ending command, shell, and tunnel activity tied to that session when shutdown is complete.

### [Export-SSHShellTranscript](Export-SSHShellTranscript.md)
Exports the interactive SSH shell transcript to a file.

Writes the shell transcript to disk for troubleshooting or audit purposes, with optional append mode and support for exporting only the newest entries from a long-running shell session.

### [Get-FTPChecksum](Get-FTPChecksum.md)
Calculates or retrieves a checksum for a remote FTP file.

Requests a remote hash from the FTP server by using the selected algorithm, which is useful for post-upload verification and drift detection workflows.

### [Get-FTPChmod](Get-FTPChmod.md)
Reads POSIX-style permission bits for a remote FTP item.

Returns the remote mode/permission information reported by the FTP server so scripts can inspect Unix-style access flags before applying changes.

### [Get-FTPFileSize](Get-FTPFileSize.md)
Returns the size of a remote FTP file.

Reads the remote file length and can fall back to a caller-supplied default value when the size cannot be determined reliably.

### [Get-FTPItem](Get-FTPItem.md)
Retrieves metadata for a single FTP or FTPS file-system item.

Returns a single remote item with file or directory metadata, optionally following symbolic links when the remote server exposes them through the FTP listing surface.

### [Get-FTPList](Get-FTPList.md)
Lists files and directories from an FTP or FTPS session.

Returns Transferetto remote item objects for a target FTP path, with optional FluentFTP listing flags for recursive, force-listing, or link-aware enumeration scenarios.

### [Get-FTPModifiedTime](Get-FTPModifiedTime.md)
Returns the last modified time for a remote FTP item.

Reads the remote timestamp reported by the FTP server, which is useful for deployment comparisons, freshness checks, and timestamp synchronization.

### [Get-FTPWorkingDirectory](Get-FTPWorkingDirectory.md)
Returns the current working directory for an FTP or FTPS session.

Exposes the session’s active remote working directory so scripts can confirm navigation state before relative listing, upload, download, or rename operations.

### [Get-SFTPChmod](Get-SFTPChmod.md)
Reads POSIX-style permission bits for a remote SFTP item.

Returns the remote mode/permission information reported by the SFTP server so scripts can inspect Unix-style access flags before applying changes.

### [Get-SFTPContent](Get-SFTPContent.md)
Reads the full content of a remote SFTP file as text or bytes.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, with support for text decoding or raw byte retrieval.

### [Get-SFTPItem](Get-SFTPItem.md)
Retrieves metadata for a single SFTP file-system item.

Returns SFTP attributes for a target path so scripts can inspect timestamps, permissions, size, and item type before taking further action.

### [Get-SFTPList](Get-SFTPList.md)
Lists files and directories from an SFTP session.

Returns Transferetto remote item objects for a target SFTP path so PowerShell scripts can inspect directory contents, filter entries, and pipe them into later file-management commands.

### [Get-SFTPWorkingDirectory](Get-SFTPWorkingDirectory.md)
Returns the current working directory for an SFTP session.

Exposes the session’s active SFTP path so scripts can coordinate relative reads, writes, and directory-management steps safely.

### [Get-SSHShellTranscript](Get-SSHShellTranscript.md)
Retrieves the in-memory transcript captured for an interactive SSH shell session.

Returns either a structured transcript snapshot or plain text entries, with optional trimming to the newest entries so long-running interactive sessions can be inspected without exporting everything.

### [Get-TransferChildItem](Get-TransferChildItem.md)
Lists items beneath a path on any Transferetto endpoint.

### [Get-TransferItem](Get-TransferItem.md)
Gets one item from any Transferetto endpoint.

### [Invoke-SSHShellCommand](Invoke-SSHShellCommand.md)
Runs a command inside an interactive SSH shell and captures output plus exit code.

Uses the reusable shell marker and prompt-handling lane to execute a command in a live shell session, optionally stream output while it runs, trim command echo, and return either structured results or raw shell output.

### [Invoke-SSHShellExpect](Invoke-SSHShellExpect.md)
Executes an ordered expect-style workflow against an interactive SSH shell.

Supports send-text, control-key, prompt, text, regex, line, idle, and follow-mode steps through reusable step objects or PSCustomObject input, making it possible to script interactive shell flows without dropping into raw shell polling logic.

### [Invoke-SSHShellRecipe](Invoke-SSHShellRecipe.md)
Runs a reusable Linux administration recipe inside an interactive SSH shell.

Provides a higher-level shell automation layer for common administration flows such as running a sudo command, following a remote file with tail, or following systemd logs with journalctl, while still supporting prompt presets, streaming output, and cancellation.

### [Move-FTPDirectory](Move-FTPDirectory.md)
Moves or renames a directory on an FTP or FTPS server.

Relocates a remote FTP directory to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.

### [Move-FTPFile](Move-FTPFile.md)
Moves or renames a file on an FTP or FTPS server.

Relocates a remote FTP file to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.

### [Move-SFTPDirectory](Move-SFTPDirectory.md)
Moves or renames a directory on an SFTP server.

Renames or relocates a remote SFTP directory, optionally using POSIX rename semantics when the server supports them, and returns a structured operation result unless suppressed.

### [Move-SFTPFile](Move-SFTPFile.md)
Moves or renames a file on an SFTP server.

Renames or relocates a remote SFTP file, optionally using POSIX rename semantics when the server supports them, and returns a structured operation result unless suppressed.

### [New-FTPDirectory](New-FTPDirectory.md)
Creates a directory on an FTP or FTPS server.

Creates a remote directory and can force parent creation when needed, returning a structured result unless output is intentionally suppressed.

### [New-SFTPDirectory](New-SFTPDirectory.md)
Creates a directory on an SFTP server.

Creates a remote SFTP directory and returns the operation result unless output is suppressed, making it easy to compose idempotent provisioning flows.

### [New-SFTPSymbolicLink](New-SFTPSymbolicLink.md)
Creates a symbolic link on an SFTP server.

Creates a remote symlink from the requested link path to the target path and can suppress the returned operation result for quieter automation.

### [New-SSHShell](New-SSHShell.md)
Creates a reusable interactive SSH shell session.

Builds an interactive shell stream on top of an SSH session, with optional prompt presets or explicit prompt patterns, transcript support, and terminal sizing that can be reused by shell read, write, expect, and recipe cmdlets.

### [New-TransferAzureBlobEndpoint](New-TransferAzureBlobEndpoint.md)
Creates an Azure Blob Transferetto endpoint.

Connects through a protected connection string or a container URI, including a container SAS URI. The endpoint performs blob data-plane operations and does not create or administer storage resources.

### [New-TransferS3Endpoint](New-TransferS3Endpoint.md)
Creates an Amazon S3 or S3-compatible Transferetto endpoint.

Uses the AWS default credential chain unless an explicit access-key credential is supplied. Custom endpoints support S3-compatible services such as MinIO, Cloudflare R2, and Backblaze B2.

### [Open-FTPStream](Open-FTPStream.md)
Opens a readable or writable FTP stream for a remote file.

Creates a reusable stream session for low-level FTP file access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

### [Open-SFTPStream](Open-SFTPStream.md)
Opens a readable or writable SFTP stream for a remote file.

Creates a reusable stream session for low-level SFTP access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

### [Read-FTPStream](Read-FTPStream.md)
Reads bytes or text from an open FTP stream session.

Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental FTP stream reads can be scripted without buffering an entire file up front.

### [Read-SFTPStream](Read-SFTPStream.md)
Reads bytes or text from an open SFTP stream session.

Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental SFTP stream reads can be automated efficiently.

### [Read-SSHShell](Read-SSHShell.md)
Reads output from an interactive SSH shell session.

Supports simple reads, line reads, read-until-idle, text and regex expectations, prompt waits, follow-mode output capture, prompt presets, progressive streaming, and cancellation-aware polling for interactive shell automation.

### [Receive-FTPDirectory](Receive-FTPDirectory.md)
Downloads a remote FTP or FTPS directory tree to the local machine.

Supports FluentFTP folder sync modes, local collision policy, optional verification rules, shared progress reporting, and cancellation-aware async directory downloads for both FTP and FTPS sessions.

### [Receive-FTPFile](Receive-FTPFile.md)
Downloads one or more files from an FTP or FTPS session to the local machine.

Supports explicit remote paths or native listing objects, local collision policy, optional verification, shared transfer progress, and cancellation-aware async downloads for both FTP and FTPS sessions.

### [Receive-SCPDirectory](Receive-SCPDirectory.md)
Downloads a remote directory tree through an SCP session.

Provides recursive SCP downloads with the same shared progress reporting and cancellation-aware async behavior used by the rest of the Transferetto file-transfer surface.

### [Receive-SCPFile](Receive-SCPFile.md)
Downloads a file through an SCP session.

Provides a simple SCP receive path with the shared Transferetto async transfer options so scripts can show progress and cancel long downloads consistently.

### [Receive-SFTPDirectory](Receive-SFTPDirectory.md)
Downloads a remote directory tree from an SFTP session.

Supports recursive SFTP downloads with overwrite control, progress reporting, and cancellation-aware async execution so local staging and backup workflows behave consistently across protocols.

### [Receive-SFTPFile](Receive-SFTPFile.md)
Downloads a file from an SFTP session to the local machine.

Uses the shared async transfer pipeline so SFTP downloads support cancellation and progress reporting consistently with the FTP, SCP, and broader Transferetto file-transfer surface.

### [Receive-TransferItem](Receive-TransferItem.md)
Downloads an item from any readable Transferetto endpoint.

### [Remove-FTPDirectory](Remove-FTPDirectory.md)
Removes a directory from an FTP or FTPS server.

Deletes a remote FTP directory and can pass explicit listing options for servers that need additional directory enumeration behavior during recursive removal.

### [Remove-FTPFile](Remove-FTPFile.md)
Removes a file from an FTP or FTPS server.

Deletes a single remote FTP file, which fits cleanup, rollback, and artifact rotation workflows.

### [Remove-SFTPDirectory](Remove-SFTPDirectory.md)
Removes a directory from an SFTP server.

Deletes a remote SFTP directory and returns the operation result unless suppressed, which fits well into cleanup and deployment-rollback scripts.

### [Remove-SFTPFile](Remove-SFTPFile.md)
Removes a file from an SFTP server.

Deletes a single remote SFTP file and can suppress the returned operation result when used inside larger maintenance scripts.

### [Remove-TransferItem](Remove-TransferItem.md)
Deletes an item from any writable Transferetto endpoint.

### [Rename-FTPFile](Rename-FTPFile.md)
Renames a remote FTP file or relocates it to a new path.

Performs a server-side rename for a remote FTP item, which is useful for finalizing staged uploads or rotating files in place.

### [Rename-SFTPFile](Rename-SFTPFile.md)
Renames a remote SFTP file or relocates it to a new path.

Performs a server-side rename for a remote SFTP item, which is useful for finalizing staged uploads or rotating files in place.

### [Request-FTPConfiguration](Request-FTPConfiguration.md)
Probes an FTP or FTPS endpoint to discover compatible connection settings.

Runs Transferetto’s FTP configuration detection against a target server, optionally with credentials, and can return either the first working configuration or the full candidate set.

### [Send-FTPDirectory](Send-FTPDirectory.md)
Uploads a local directory tree to an FTP or FTPS session.

Supports FluentFTP folder sync modes, remote collision policy, verification, transfer rules, shared progress reporting, and cancellation-aware async directory uploads for both FTP and FTPS targets.

### [Send-FTPFile](Send-FTPFile.md)
Uploads one or more local files to an FTP or FTPS session.

Supports explicit remote targets or automatic filename mapping, remote collision policy, optional verification, remote directory creation, shared transfer progress, and cancellation-aware async uploads for both FTP and FTPS sessions.

### [Send-SCPDirectory](Send-SCPDirectory.md)
Uploads a local directory tree through an SCP session.

Supports recursive SCP uploads with shared progress reporting and cancellation-aware async execution, making it suitable for simple release and backup flows that do not need SFTP-specific metadata operations.

### [Send-SCPFile](Send-SCPFile.md)
Uploads a local file through an SCP session.

Provides a simple SCP upload path with the shared Transferetto async transfer options so scripts can show progress and cancel long uploads consistently.

### [Send-SFTPDirectory](Send-SFTPDirectory.md)
Uploads a local directory tree to an SFTP session.

Uses the shared Transferetto async transfer pipeline so recursive SFTP uploads support overwrite control, progress reporting, and cancellation consistently with the FTP and SCP directory transfer cmdlets.

### [Send-SFTPFile](Send-SFTPFile.md)
Uploads a local file to an SFTP session.

Supports overwrite control, shared transfer progress reporting, and cancellation-aware async uploads that can be reused in deployment and automation workflows.

### [Send-SSHCommand](Send-SSHCommand.md)
Runs one or more non-interactive SSH commands and captures their output.

Supports multi-line command blocks, structured status results, progressive stdout and stderr streaming, and per-command timeouts on top of the reusable SSH command execution layer.

### [Send-SSHShellControl](Send-SSHShellControl.md)
Sends control-key input to an interactive SSH shell session.

Provides a safe way to send interrupt and navigation keys such as Ctrl+C or Ctrl+D without embedding terminal escape sequences directly into shell automation scripts.

### [Send-TransferItem](Send-TransferItem.md)
Uploads a local file to any writable Transferetto endpoint.

### [Set-FTPChmod](Set-FTPChmod.md)
Sets POSIX-style permissions for a remote FTP item.

Supports both octal-style integer permission values and explicit owner/group/other permission flags, depending on which representation is more convenient for the caller.

### [Set-FTPModifiedTime](Set-FTPModifiedTime.md)
Sets the last modified time for a remote FTP item.

Writes a remote timestamp and can optionally return the updated item metadata, which is helpful for preserving deployment timestamps after upload.

### [Set-FTPOption](Set-FTPOption.md)
Adjusts runtime options on an existing FTP session.

Lets scripts fine-tune retry behavior and zero-byte download handling on a live session without reconnecting.

### [Set-FTPStreamPosition](Set-FTPStreamPosition.md)
Moves the current position within an open FTP stream session.

Seeks to a new offset in the FTP stream so callers can reread, skip ahead, or resume low-level stream-based operations from a specific location.

### [Set-FTPTracing](Set-FTPTracing.md)
Enables or disables global FTP protocol tracing for the current PowerShell session.

Configures diagnostic logging visibility for usernames, passwords, and hosts so troubleshooting can be more detailed or more redacted depending on the scenario.

### [Set-FTPWorkingDirectory](Set-FTPWorkingDirectory.md)
Changes the working directory for an FTP or FTPS session.

Updates the session’s active FTP path so later relative operations run against the intended remote location.

### [Set-SFTPChmod](Set-SFTPChmod.md)
Sets POSIX-style permissions for a remote SFTP item.

Supports symbolic permission strings or explicit owner/group/other digit values and can optionally return refreshed item metadata after the change.

### [Set-SFTPContent](Set-SFTPContent.md)
Writes text or bytes to a remote SFTP file.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, supporting text writes, append mode, or raw byte content.

### [Set-SFTPStreamPosition](Set-SFTPStreamPosition.md)
Moves the current position within an open SFTP stream session.

Seeks to a new offset in the SFTP stream so callers can reread, skip ahead, or resume low-level stream-based operations from a specific location.

### [Set-SFTPTimestamp](Set-SFTPTimestamp.md)
Sets access and/or write timestamps for a remote SFTP item.

Updates one or both SFTP timestamps, with optional UTC semantics, and can return refreshed item metadata after the change.

### [Set-SFTPWorkingDirectory](Set-SFTPWorkingDirectory.md)
Changes the working directory for an SFTP session.

Updates the session’s active SFTP path so later relative operations run against the intended remote location.

### [Set-SSHShellPrompt](Set-SSHShellPrompt.md)
Updates the prompt detection settings for an interactive SSH shell session.

Configures either an explicit prompt regex or a reusable prompt preset so later read, expect, and command cmdlets can synchronize against the correct shell prompt.

### [Set-SSHShellSize](Set-SSHShellSize.md)
Resizes the virtual terminal backing an interactive SSH shell session.

Updates terminal rows, columns, and optional pixel dimensions so interactive programs such as editors, pagers, or full-screen tools render correctly in the remote shell.

### [Start-FXPDirectoryTransfer](Start-FXPDirectoryTransfer.md)
Transfers a directory tree directly between two FTP/FTPS servers by using FXP.

Starts a server-to-server directory sync through the reusable Transferetto FXP layer, with folder sync mode, verification, rules, collision handling, and progress reporting.

### [Start-FXPFileTransfer](Start-FXPFileTransfer.md)
Transfers a file directly between two FTP/FTPS servers by using FXP.

Starts a server-to-server file copy through the reusable Transferetto FXP layer, with remote collision handling, optional verification, destination directory creation, and progress reporting.

### [Start-SSHLocalTunnel](Start-SSHLocalTunnel.md)
Starts a local SSH port-forwarding tunnel.

Binds a local host and port, then forwards traffic through the SSH session to a remote host and port, returning a reusable tunnel session that can be stopped later.

### [Start-SSHRemoteTunnel](Start-SSHRemoteTunnel.md)
Starts a remote SSH port-forwarding tunnel.

Requests the SSH server to bind a remote host and port, then forwards traffic back through the SSH session to a target host and port reachable from the client side.

### [Stop-SSHShellCommand](Stop-SSHShellCommand.md)
Stops a running interactive SSH shell command and waits for the prompt to return.

Uses the shell stop lane to interrupt the active command, optionally waiting for a resolved prompt pattern or preset before returning the captured stop result.

### [Stop-SSHTunnel](Stop-SSHTunnel.md)
Stops an SSH tunnel session.

Closes a tunnel created by Start-SSHLocalTunnel or Start-SSHRemoteTunnel, releasing the forwarded port cleanly.

### [Sync-FTPDirectory](Sync-FTPDirectory.md)
Synchronizes a local directory with an FTP or FTPS directory.

Uses the shared Transferetto synchronization planner to upload or download missing and changed files, optionally mirror-delete extra destination items, filter paths by wildcard patterns, preserve timestamps, and preview planned work with dry-run mode.

### [Sync-FTPStream](Sync-FTPStream.md)
Flushes buffered writes for an open FTP stream session.

Forces pending FTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.

### [Sync-SFTPDirectory](Sync-SFTPDirectory.md)
Synchronizes a local directory with an SFTP directory.

Uses the shared Transferetto synchronization planner to upload or download missing and changed files, optionally mirror-delete extra destination items, filter paths by wildcard patterns, preserve timestamps, and preview planned work with dry-run mode.

### [Sync-SFTPStream](Sync-SFTPStream.md)
Flushes buffered writes for an open SFTP stream session.

Forces pending SFTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.

### [Test-FTPDirectory](Test-FTPDirectory.md)
Checks whether a remote FTP directory exists.

Returns a Boolean-like existence result for a remote FTP directory path, which is useful before create, remove, or sync operations.

### [Test-FTPFile](Test-FTPFile.md)
Checks whether a remote FTP file exists.

Returns a Boolean-like existence result for a remote FTP file path, which is useful for guard clauses and idempotent deployment flows.

### [Test-FXPTransfer](Test-FXPTransfer.md)
Preflights whether an FXP transfer can run between two FTP/FTPS sessions.

Evaluates the requested source, destination, transfer kind, and optional destination-directory creation rules before a full FXP transfer is attempted.

### [Test-SFTPDirectory](Test-SFTPDirectory.md)
Checks whether a remote SFTP directory exists.

Returns a Boolean-like existence result for a remote SFTP directory path, which is useful before create, remove, or sync operations.

### [Test-SFTPFile](Test-SFTPFile.md)
Checks whether a remote SFTP file exists.

Returns a Boolean-like existence result for a remote SFTP file path, which is useful for guard clauses and idempotent deployment flows.

### [Test-SFTPPath](Test-SFTPPath.md)
Checks whether a remote SFTP path exists.

Returns a Boolean-like existence result for a remote SFTP path regardless of whether it is a file, directory, or other supported item type.

### [Test-SFTPSymbolicLink](Test-SFTPSymbolicLink.md)
Checks whether a remote SFTP symbolic link exists.

Returns a Boolean-like existence result specifically for a symbolic-link path so scripts can distinguish link-oriented workflows from file or directory checks.

### [Test-TransferItem](Test-TransferItem.md)
Tests whether an item exists on any Transferetto endpoint.

### [Wait-SSHShellPrompt](Wait-SSHShellPrompt.md)
Waits until an expected interactive SSH shell prompt is observed.

Supports explicit prompt regexes or reusable prompt presets, progressive streaming while waiting, and cancellation-aware polling so shell automation can synchronize reliably before the next interactive step.

### [Write-FTPStream](Write-FTPStream.md)
Writes text or bytes to an open FTP stream session.

Supports text encoding or raw byte writes, optional flush behavior, and progress-aware async execution for low-level FTP upload scenarios that need incremental control.

### [Write-SFTPStream](Write-SFTPStream.md)
Writes text or bytes to an open SFTP stream session.

Supports text encoding or raw byte writes, optional flush behavior, and progress-aware async execution for low-level SFTP upload or remote content-editing scenarios.

### [Write-SSHShell](Write-SSHShell.md)
Writes text into an interactive SSH shell session.

Sends raw text or line-based input to an existing shell stream, with optional newline suppression and pass-through support so command composition can stay in PowerShell pipelines.
