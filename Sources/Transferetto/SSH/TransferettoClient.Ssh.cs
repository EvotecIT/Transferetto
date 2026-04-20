using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Transferetto;
/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>

public static partial class TransferettoClient {
    /// <summary>
    /// Creates and connects an SSH session.
    /// </summary>
    public static TransferettoSshSession ConnectSsh(TransferettoSshConnectionOptions options) {
        EnsureNotNull(options, nameof(options));
        EnsureNotNullOrWhiteSpace(options.Server, nameof(options.Server));
        ValidateSshHostKeyTrustOptions(options);
        ValidateSshProxyOptions(options);

        SshClient client = CreateSshClient(options);
        TransferettoSshHostKeyInfo? hostKeyInfo = null;
        client.HostKeyReceived += (_, args) => {
            hostKeyInfo = EvaluateHostKeyTrust(options, args);
            args.CanTrust = hostKeyInfo.CanTrust;
        };

        try {
            ApplySshClientOptions(client, options);
            client.Connect();
            return new TransferettoSshSession(client) {
                HostKeyInfo = hostKeyInfo
            };
        } catch {
            client.Dispose();
            throw;
        }
    }
    /// <summary>
    /// Runs one or more non-interactive SSH commands.
    /// </summary>

    public static TransferettoSshCommandResult SendSshCommand(TransferettoSshSession session, IEnumerable<string> commands) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNull(commands, nameof(commands));

        string commandText = string.Join(string.Empty, commands
            .Where(static command => !string.IsNullOrWhiteSpace(command))
            .Select(static command => command.TrimEnd().EndsWith(";") ? command : $"{command};"));

        SshCommand command = session.Client.CreateCommand(commandText);
        string output = command.Execute();
        return new TransferettoSshCommandResult {
            Status = command.ExitStatus == 0,
            Output = output,
            Error = string.IsNullOrWhiteSpace(command.Error) ? null : command.Error
        };
    }
    /// <summary>
    /// Closes an SSH session.
    /// </summary>

    public static void DisconnectSsh(TransferettoSshSession session) {
        EnsureNotNull(session, nameof(session));
        if (session.Client.IsConnected) {
            session.Client.Disconnect();
        }
    }
    /// <summary>
    /// Creates an interactive SSH shell session.
    /// </summary>

    public static TransferettoSshShellSession CreateSshShell(TransferettoSshSession session, TransferettoSshShellOptions? options = null) {
        EnsureNotNull(session, nameof(session));

        TransferettoSshShellOptions resolvedOptions = options ?? new TransferettoSshShellOptions();
        ShellStream shellStream = resolvedOptions.NoTerminal
            ? session.Client.CreateShellStreamNoTerminal(resolvedOptions.BufferSize)
            : session.Client.CreateShellStream(
                resolvedOptions.TerminalName,
                resolvedOptions.Columns,
                resolvedOptions.Rows,
                resolvedOptions.Width,
                resolvedOptions.Height,
                resolvedOptions.BufferSize);

        return new TransferettoSshShellSession(session, shellStream, resolvedOptions);
    }
    /// <summary>
    /// Closes an interactive SSH shell session.
    /// </summary>

    public static void CloseSshShell(TransferettoSshShellSession session) {
        EnsureNotNull(session, nameof(session));
        session.Dispose();
    }
    /// <summary>
    /// Reads text from an interactive SSH shell session.
    /// </summary>

    public static string ReadSshShell(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        bool readLine = false,
        string? expectText = null,
        int lookback = -1,
        string? regexPattern = null,
        bool readUntilIdle = false,
        TimeSpan? idleTimeout = null,
        bool expectPrompt = false,
        string? promptPattern = null) {
        EnsureNotNull(session, nameof(session));

        if (!string.IsNullOrWhiteSpace(regexPattern)) {
            return ReadSshShellRegex(session, regexPattern!, timeout, lookback);
        }

        if (expectPrompt || !string.IsNullOrWhiteSpace(promptPattern)) {
            return WaitForSshShellPrompt(session, timeout, promptPattern, lookback);
        }

        if (readUntilIdle) {
            return ReadSshShellUntilIdle(session, idleTimeout ?? TimeSpan.FromMilliseconds(500), timeout);
        }

        if (!string.IsNullOrWhiteSpace(expectText)) {
            string expectedText = expectText!;
            string output = timeout.HasValue
                ? session.ShellStream.Expect(expectedText, timeout.Value, lookback) ?? string.Empty
                : session.ShellStream.Expect(expectedText) ?? string.Empty;
            session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
            return output;
        }

        if (readLine) {
            string output = timeout.HasValue
                ? session.ShellStream.ReadLine(timeout.Value) ?? string.Empty
                : session.ShellStream.ReadLine() ?? string.Empty;
            session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
            return output;
        }

        if (timeout.HasValue) {
            string output = ReadSshShellWithTimeout(session.ShellStream, timeout.Value);
            session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
            return output;
        }

        string immediateOutput = session.ShellStream.DataAvailable ? session.ShellStream.Read() : string.Empty;
        session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, immediateOutput);
        return immediateOutput;
    }
    /// <summary>
    /// Reads SSH shell output until a regular expression matches.
    /// </summary>

    public static string ReadSshShellRegex(
        TransferettoSshShellSession session,
        string pattern,
        TimeSpan? timeout = null,
        int lookback = -1) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(pattern, nameof(pattern));

        Regex regex = new(pattern, RegexOptions.Multiline);
        return ReadSshShellUntilRegexMatch(session, regex, timeout, lookback);
    }
    /// <summary>
    /// Reads SSH shell output until it becomes idle.
    /// </summary>

    public static string ReadSshShellUntilIdle(
        TransferettoSshShellSession session,
        TimeSpan idleTimeout,
        TimeSpan? timeout = null) {
        EnsureNotNull(session, nameof(session));

        if (idleTimeout <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(idleTimeout), "Idle timeout must be greater than zero.");
        }

        Stopwatch overallStopwatch = Stopwatch.StartNew();
        Stopwatch idleStopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();
        bool receivedAnyData = false;

        while (true) {
            if (session.ShellStream.DataAvailable) {
                builder.Append(session.ShellStream.Read());
                receivedAnyData = true;
                idleStopwatch.Restart();
                continue;
            }

            if (receivedAnyData && idleStopwatch.Elapsed >= idleTimeout) {
                string output = builder.ToString();
                session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
                return output;
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && overallStopwatch.Elapsed >= timeout.Value) {
                string output = builder.ToString();
                session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
                return output;
            }

            Thread.Sleep(50);
        }
    }
    /// <summary>
    /// Writes text to an interactive SSH shell session.
    /// </summary>

    public static void WriteSshShell(TransferettoSshShellSession session, string? text, bool appendLine = true) {
        EnsureNotNull(session, nameof(session));

        string content = text ?? string.Empty;
        if (appendLine) {
            session.ShellStream.WriteLine(content);
            session.RecordTranscript(TransferettoSshShellTranscriptDirection.Write, content + Environment.NewLine);
        } else {
            session.ShellStream.Write(content);
            session.RecordTranscript(TransferettoSshShellTranscriptDirection.Write, content);
        }
    }
    /// <summary>
    /// Sends a control key to an interactive SSH shell session.
    /// </summary>

    public static void SendSshShellControl(
        TransferettoSshShellSession session,
        TransferettoSshShellControlKey controlKey,
        int repeat = 1) {
        EnsureNotNull(session, nameof(session));

        if (repeat <= 0) {
            throw new ArgumentOutOfRangeException(nameof(repeat), "Repeat must be greater than zero.");
        }

        string controlText = string.Concat(Enumerable.Repeat(GetSshShellControlText(controlKey), repeat));
        session.ShellStream.Write(controlText);
        session.ShellStream.Flush();
        session.RecordTranscript(TransferettoSshShellTranscriptDirection.Control, repeat == 1 ? $"<{controlKey}>" : $"<{controlKey} x{repeat}>");
    }
    /// <summary>
    /// Reads and clears buffered SSH shell output.
    /// </summary>

    public static string ClearSshShellBuffer(TransferettoSshShellSession session) {
        EnsureNotNull(session, nameof(session));

        StringBuilder builder = new();
        while (session.ShellStream.DataAvailable) {
            builder.Append(session.ShellStream.Read());
        }

        string output = builder.ToString();
        session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
        return output;
    }
    /// <summary>
    /// Gets the current SSH shell transcript snapshot.
    /// </summary>

    public static TransferettoSshShellTranscriptSnapshot GetSshShellTranscript(
        TransferettoSshShellSession session,
        int? lastEntries = null) {
        EnsureNotNull(session, nameof(session));
        return session.GetTranscript(lastEntries);
    }
    /// <summary>
    /// Clears the SSH shell transcript buffer.
    /// </summary>

    public static TransferettoSshShellTranscriptSnapshot ClearSshShellTranscript(TransferettoSshShellSession session) {
        EnsureNotNull(session, nameof(session));
        return session.ClearTranscript();
    }
    /// <summary>
    /// Exports the SSH shell transcript to disk.
    /// </summary>

    public static TransferettoOperationResult ExportSshShellTranscript(
        TransferettoSshShellSession session,
        string path,
        bool append = false,
        int? lastEntries = null) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        TransferettoSshShellTranscriptSnapshot snapshot = GetSshShellTranscript(session, lastEntries);
        string content = FormatSshShellTranscript(snapshot);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        if (append) {
            File.AppendAllText(path, content);
        } else {
            File.WriteAllText(path, content);
        }

        return new TransferettoOperationResult {
            Action = "ExportShellTranscript",
            Status = true,
            Message = $"Exported {snapshot.CapturedEntryCount} transcript entries.",
            Path = path
        };
    }
    /// <summary>
    /// Stops a running SSH shell command and collects trailing output.
    /// </summary>

    public static string StopSshShellCommand(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        int lookback = -1) {
        EnsureNotNull(session, nameof(session));

        SendSshShellControl(session, TransferettoSshShellControlKey.Interrupt);

        string? resolvedPromptPattern = !string.IsNullOrWhiteSpace(promptPattern)
            ? promptPattern
            : session.PromptPattern;

        if (!string.IsNullOrWhiteSpace(resolvedPromptPattern)) {
            return WaitForSshShellPrompt(
                session,
                timeout ?? TimeSpan.FromSeconds(5),
                resolvedPromptPattern,
                lookback);
        }

        return ReadSshShellUntilIdle(
            session,
            TimeSpan.FromMilliseconds(500),
            timeout ?? TimeSpan.FromSeconds(5));
    }
    /// <summary>
    /// Updates the expected prompt pattern for an SSH shell session.
    /// </summary>

    public static void SetSshShellPromptPattern(TransferettoSshShellSession session, string? promptPattern) {
        EnsureNotNull(session, nameof(session));
        session.UpdatePromptPattern(promptPattern);
    }
    /// <summary>
    /// Waits for an SSH shell prompt to appear.
    /// </summary>

    public static string WaitForSshShellPrompt(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        int lookback = -1) {
        EnsureNotNull(session, nameof(session));

        string? resolvedPattern = !string.IsNullOrWhiteSpace(promptPattern)
            ? promptPattern
            : session.PromptPattern;

        if (string.IsNullOrWhiteSpace(resolvedPattern)) {
            throw new InvalidOperationException("No SSH shell prompt pattern was configured.");
        }

        Regex regex = new(resolvedPattern, RegexOptions.Multiline);
        return ReadSshShellUntilRegexMatch(session, regex, timeout, lookback);
    }
    /// <summary>
    /// Runs a command through an interactive SSH shell and captures the result.
    /// </summary>

    public static TransferettoSshShellCommandResult InvokeSshShellCommand(
        TransferettoSshShellSession session,
        string command,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        bool trimCommandEcho = true,
        int lookback = -1) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(command, nameof(command));

        string marker = "__TRANSFERETTO__" + Guid.NewGuid().ToString("N");
        WriteSshShell(session, command, appendLine: true);
        WriteSshShell(session, $"printf '{marker}:%s\\n' $?", appendLine: true);

        Regex markerRegex = new($"(?m)^{Regex.Escape(marker)}:(-?\\d+)\\r?$", RegexOptions.Multiline);
        string output = ReadSshShellUntilRegexMatch(session, markerRegex, timeout, lookback);
        Match markerMatch = markerRegex.Match(output);
        int? exitCode = null;
        if (markerMatch.Success && int.TryParse(markerMatch.Groups[1].Value, out int parsedExitCode)) {
            exitCode = parsedExitCode;
        }

        string cleanedOutput = markerRegex.Replace(output, string.Empty).TrimEnd();
        if (trimCommandEcho) {
            cleanedOutput = TrimLeadingCommandEcho(cleanedOutput, command);
        }

        string? resolvedPromptPattern = !string.IsNullOrWhiteSpace(promptPattern)
            ? promptPattern
            : session.PromptPattern;

        if (!string.IsNullOrWhiteSpace(resolvedPromptPattern)) {
            Regex promptRegex = new(resolvedPromptPattern, RegexOptions.Multiline);
            string trailingAfterMarker = markerMatch.Success
                ? output.Substring(markerMatch.Index + markerMatch.Length)
                : string.Empty;

            if (!promptRegex.IsMatch(trailingAfterMarker)) {
                string trailingOutput = WaitForSshShellPrompt(
                    session,
                    timeout ?? TimeSpan.FromSeconds(5),
                    resolvedPromptPattern,
                    lookback);
                if (!string.IsNullOrEmpty(trailingOutput)) {
                    cleanedOutput = string.IsNullOrEmpty(cleanedOutput)
                        ? trailingOutput.TrimEnd()
                        : (cleanedOutput + Environment.NewLine + trailingOutput.TrimEnd()).TrimEnd();
                }
            }
        }

        return new TransferettoSshShellCommandResult {
            Command = command,
            Output = cleanedOutput,
            ExitCode = exitCode,
            Status = exitCode.GetValueOrDefault() == 0,
            Marker = marker,
            PromptPattern = resolvedPromptPattern
        };
    }
    /// <summary>
    /// Resizes the pseudo-terminal for an SSH shell session.
    /// </summary>

    public static void ResizeSshShell(TransferettoSshShellSession session, uint columns, uint rows, uint width, uint height) {
        EnsureNotNull(session, nameof(session));

        if (session.NoTerminal) {
            throw new InvalidOperationException("Cannot resize a shell created without a pseudo-terminal.");
        }

        session.ShellStream.ChangeWindowSize(columns, rows, width, height);
        session.UpdateWindowSize(columns, rows, width, height);
    }
    /// <summary>
    /// Starts a local SSH tunnel.
    /// </summary>

    public static TransferettoSshTunnelSession StartSshLocalTunnel(
        TransferettoSshSession session,
        uint boundPort,
        string remoteHost,
        uint remotePort,
        string boundHost = "127.0.0.1") {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(remoteHost, nameof(remoteHost));
        EnsureNotNullOrWhiteSpace(boundHost, nameof(boundHost));

        ForwardedPortLocal forwardedPort = new(boundHost, boundPort, remoteHost, remotePort);
        session.Client.AddForwardedPort(forwardedPort);
        forwardedPort.Start();

        return new TransferettoSshTunnelSession(session, forwardedPort, TransferettoSshTunnelType.Local, boundHost, forwardedPort.BoundPort, remoteHost, remotePort);
    }
    /// <summary>
    /// Starts a remote SSH tunnel.
    /// </summary>

    public static TransferettoSshTunnelSession StartSshRemoteTunnel(
        TransferettoSshSession session,
        uint boundPort,
        string targetHost,
        uint targetPort,
        string boundHost = "127.0.0.1") {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(targetHost, nameof(targetHost));
        EnsureNotNullOrWhiteSpace(boundHost, nameof(boundHost));

        ForwardedPortRemote forwardedPort = new(boundHost, boundPort, targetHost, targetPort);
        session.Client.AddForwardedPort(forwardedPort);
        forwardedPort.Start();

        return new TransferettoSshTunnelSession(session, forwardedPort, TransferettoSshTunnelType.Remote, boundHost, forwardedPort.BoundPort, targetHost, targetPort);
    }
    /// <summary>
    /// Stops an SSH tunnel session.
    /// </summary>

    public static void StopSshTunnel(TransferettoSshTunnelSession session) {
        EnsureNotNull(session, nameof(session));
        session.Stop();
    }

    private static SshClient CreateSshClient(TransferettoSshConnectionOptions options) {
        ConnectionInfo connectionInfo = CreateSshConnectionInfo(options);
        return new SshClient(connectionInfo);
    }

    private static void ValidateSshHostKeyTrustOptions(TransferettoSshConnectionOptions options) {
        bool hasExpectedFingerprints = options.ExpectedHostKeyFingerprints?.Any(static value => !string.IsNullOrWhiteSpace(value)) == true;
        if (options.AcceptAnyHostKey && hasExpectedFingerprints) {
            throw new InvalidOperationException("AcceptAnyHostKey cannot be combined with ExpectedHostKeyFingerprints.");
        }
    }

    private static void ValidateSshProxyOptions(TransferettoSshConnectionOptions options) {
        if (options.ProxyType == TransferettoSshProxyType.None) {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.ProxyHost)) {
            throw new InvalidOperationException("ProxyHost must be provided when ProxyType is enabled.");
        }

        if (!options.ProxyPort.HasValue || options.ProxyPort.Value <= 0) {
            throw new InvalidOperationException("ProxyPort must be a positive value when ProxyType is enabled.");
        }
    }

    private static string ReadSshShellWithTimeout(ShellStream shellStream, TimeSpan timeout) {
        if (timeout == Timeout.InfiniteTimeSpan) {
            while (!shellStream.DataAvailable) {
                Thread.Sleep(50);
            }

            return shellStream.Read();
        }

        if (timeout < TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative or Timeout.InfiniteTimeSpan.");
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!shellStream.DataAvailable) {
            if (stopwatch.Elapsed >= timeout) {
                return string.Empty;
            }

            Thread.Sleep(50);
        }

        return shellStream.Read();
    }

    private static string ReadSshShellUntilRegexMatch(
        TransferettoSshShellSession session,
        Regex regex,
        TimeSpan? timeout,
        int lookback) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();
        while (true) {
            if (session.ShellStream.DataAvailable) {
                builder.Append(session.ShellStream.Read());
                string current = builder.ToString();
                string search = ApplyLookback(current, lookback);
                if (regex.IsMatch(search)) {
                    session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, current);
                    return current;
                }
            } else if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && stopwatch.Elapsed >= timeout.Value) {
                string output = builder.ToString();
                session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, output);
                return output;
            }

            Thread.Sleep(50);
        }
    }

    private static string GetSshShellControlText(TransferettoSshShellControlKey controlKey) {
        return controlKey switch {
            TransferettoSshShellControlKey.Interrupt => "\u0003",
            TransferettoSshShellControlKey.EndOfTransmission => "\u0004",
            TransferettoSshShellControlKey.Suspend => "\u001A",
            TransferettoSshShellControlKey.EndOfText => "\u0003",
            TransferettoSshShellControlKey.Escape => "\u001B",
            TransferettoSshShellControlKey.Tab => "\t",
            TransferettoSshShellControlKey.Enter => "\r",
            _ => throw new ArgumentOutOfRangeException(nameof(controlKey), controlKey, "Unsupported SSH shell control key.")
        };
    }

    private static string FormatSshShellTranscript(TransferettoSshShellTranscriptSnapshot snapshot) {
        StringBuilder builder = new();
        foreach (TransferettoSshShellTranscriptEntry entry in snapshot.Entries) {
            builder.Append('[')
                .Append(entry.TimestampUtc.ToString("O"))
                .Append("] [")
                .Append(entry.Direction)
                .AppendLine("]");
            builder.AppendLine(entry.Text);
        }

        if (snapshot.IsTruncated) {
            builder.Append("[Transcript] Dropped ")
                .Append(snapshot.DroppedEntryCount)
                .AppendLine(" older entries.");
        }

        return builder.ToString();
    }

    private static ConnectionInfo CreateSshConnectionInfo(TransferettoSshConnectionOptions options) {
        string userName = ResolveSshUserName(options);
        string? password = ResolveSshPassword(options);
        List<AuthenticationMethod> authenticationMethods = new();

        if (!string.IsNullOrWhiteSpace(options.PrivateKeyPath)) {
            string privateKeyPath = options.PrivateKeyPath ?? throw new InvalidOperationException("SSH private key path was not provided.");
            EnsureFileExists(privateKeyPath, nameof(options.PrivateKeyPath));
            PrivateKeyFile privateKeyFile = string.IsNullOrEmpty(options.PrivateKeyPassphrase)
                ? new PrivateKeyFile(privateKeyPath)
                : new PrivateKeyFile(privateKeyPath, options.PrivateKeyPassphrase);
            authenticationMethods.Add(new PrivateKeyAuthenticationMethod(userName, privateKeyFile));
        }

        if (password is not null) {
            authenticationMethods.Add(new PasswordAuthenticationMethod(userName, password));

            if (options.EnableKeyboardInteractive) {
                KeyboardInteractiveAuthenticationMethod keyboardInteractive = new(userName);
                keyboardInteractive.AuthenticationPrompt += (_, args) => {
                    foreach (AuthenticationPrompt prompt in args.Prompts) {
                        prompt.Response = password;
                    }
                };
                authenticationMethods.Add(keyboardInteractive);
            }
        }

        if (authenticationMethods.Count == 0) {
            throw new InvalidOperationException("No SSH authentication method was provided.");
        }

        int port = options.Port ?? 22;
        return HasProxy(options)
            ? new ConnectionInfo(
                options.Server,
                port,
                userName,
                MapProxyType(options.ProxyType),
                options.ProxyHost!,
                options.ProxyPort ?? 0,
                options.ProxyCredential?.UserName,
                options.ProxyCredential?.Password,
                authenticationMethods.ToArray())
            : new ConnectionInfo(options.Server, port, userName, authenticationMethods.ToArray());
    }

    private static void ApplySshClientOptions(SshClient client, TransferettoSshConnectionOptions options) {
        if (options.KeepAliveIntervalSeconds.HasValue) {
            client.KeepAliveInterval = TimeSpan.FromSeconds(options.KeepAliveIntervalSeconds.Value);
        }

        if (options.ConnectionTimeoutSeconds.HasValue) {
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds.Value);
        }

        if (options.RetryAttempts.HasValue) {
            client.ConnectionInfo.RetryAttempts = options.RetryAttempts.Value;
        }
    }

    private static string ResolveSshUserName(TransferettoSshConnectionOptions options) {
        string? userName = !string.IsNullOrWhiteSpace(options.UserName)
            ? options.UserName
            : options.Credential?.UserName;

        if (string.IsNullOrWhiteSpace(userName)) {
            throw new InvalidOperationException("SSH authentication requires UserName or Credential.");
        }

        return userName!;
    }

    private static string? ResolveSshPassword(TransferettoSshConnectionOptions options) {
        return options.Password ?? options.Credential?.Password;
    }

    private static bool HasProxy(TransferettoSshConnectionOptions options) {
        return options.ProxyType != TransferettoSshProxyType.None && !string.IsNullOrWhiteSpace(options.ProxyHost);
    }

    private static ProxyTypes MapProxyType(TransferettoSshProxyType proxyType) {
        return proxyType switch {
            TransferettoSshProxyType.Http => ProxyTypes.Http,
            TransferettoSshProxyType.Socks4 => ProxyTypes.Socks4,
            TransferettoSshProxyType.Socks5 => ProxyTypes.Socks5,
            _ => ProxyTypes.None
        };
    }

    private static string NormalizeFingerprint(string? fingerprint) {
        string normalized = (fingerprint ?? string.Empty).Trim();

        if (normalized.StartsWith("SHA256:", StringComparison.OrdinalIgnoreCase)) {
            normalized = normalized.Substring("SHA256:".Length);
        } else if (normalized.StartsWith("SHA256", StringComparison.OrdinalIgnoreCase)) {
            normalized = normalized.Substring("SHA256".Length);
        } else if (normalized.StartsWith("MD5:", StringComparison.OrdinalIgnoreCase)) {
            normalized = normalized.Substring("MD5:".Length);
        } else if (normalized.StartsWith("MD5", StringComparison.OrdinalIgnoreCase)) {
            normalized = normalized.Substring("MD5".Length);
        }

        return normalized
            .Replace(":", string.Empty)
            .Replace("-", string.Empty)
            .Replace("=", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private static TransferettoSshHostKeyInfo EvaluateHostKeyTrust(TransferettoSshConnectionOptions options, HostKeyEventArgs args) {
        TransferettoSshHostKeyInfo hostKeyInfo = CreateHostKeyInfo(args);

        if (options.AcceptAnyHostKey) {
            hostKeyInfo.CanTrust = true;
            hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.AcceptAny;
            return hostKeyInfo;
        }

        string[] expectedFingerprints = options.ExpectedHostKeyFingerprints?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeFingerprint)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? Array.Empty<string>();

        if (expectedFingerprints.Length > 0) {
            hostKeyInfo.CanTrust = FingerprintMatches(expectedFingerprints, hostKeyInfo);
            hostKeyInfo.TrustSource = hostKeyInfo.CanTrust
                ? TransferettoSshHostKeyTrustSource.ExpectedFingerprint
                : TransferettoSshHostKeyTrustSource.None;
            return hostKeyInfo;
        }

        switch (options.HostKeyPolicy) {
            case TransferettoSshHostKeyPolicy.Loose:
                hostKeyInfo.CanTrust = true;
                hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.Loose;
                return hostKeyInfo;
            case TransferettoSshHostKeyPolicy.KnownHosts:
                return EvaluateKnownHostsTrust(options, hostKeyInfo, false);
            case TransferettoSshHostKeyPolicy.TrustOnFirstUse:
            default:
                return EvaluateKnownHostsTrust(options, hostKeyInfo, true);
        }
    }

    private static TransferettoSshHostKeyInfo EvaluateKnownHostsTrust(
        TransferettoSshConnectionOptions options,
        TransferettoSshHostKeyInfo hostKeyInfo,
        bool trustOnFirstUse) {
        string knownHostsPath = ResolveKnownHostsPath(options);
        hostKeyInfo.KnownHostsPath = knownHostsPath;

        List<TransferettoSshKnownHostEntry> entries = LoadKnownHosts(knownHostsPath);
        TransferettoSshKnownHostEntry[] matchingEntries = entries
            .Where(entry => string.Equals(entry.Host, options.Server, StringComparison.OrdinalIgnoreCase) && entry.Port == (options.Port ?? 22))
            .ToArray();

        if (matchingEntries.Length == 0) {
            if (!trustOnFirstUse) {
                hostKeyInfo.CanTrust = false;
                hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.None;
                return hostKeyInfo;
            }

            entries.Add(CreateKnownHostEntry(options, hostKeyInfo));
            SaveKnownHosts(knownHostsPath, entries);
            hostKeyInfo.CanTrust = true;
            hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.TrustOnFirstUse;
            hostKeyInfo.WasPersisted = true;
            return hostKeyInfo;
        }

        TransferettoSshKnownHostEntry? trustedEntry = matchingEntries.FirstOrDefault(entry => KnownHostMatches(entry, hostKeyInfo));
        bool isTrusted = trustedEntry is not null;
        if (trustedEntry is not null) {
            trustedEntry.LastSeenUtc = DateTime.UtcNow.ToString("O");
            SaveKnownHosts(knownHostsPath, entries);
        }

        hostKeyInfo.CanTrust = isTrusted;
        hostKeyInfo.TrustSource = isTrusted
            ? TransferettoSshHostKeyTrustSource.KnownHosts
            : TransferettoSshHostKeyTrustSource.None;
        return hostKeyInfo;
    }

    private static TransferettoSshHostKeyInfo CreateHostKeyInfo(HostKeyEventArgs args) {
        return new TransferettoSshHostKeyInfo {
            HostKeyName = args.HostKeyName,
            KeyLength = args.KeyLength,
            FingerPrintMD5 = args.FingerPrintMD5,
            FingerPrintSHA256 = args.FingerPrintSHA256
        };
    }

    private static bool FingerprintMatches(IEnumerable<string> expectedFingerprints, TransferettoSshHostKeyInfo hostKeyInfo) {
        string normalizedMd5 = NormalizeFingerprint(hostKeyInfo.FingerPrintMD5);
        string normalizedSha256 = NormalizeFingerprint(hostKeyInfo.FingerPrintSHA256);

        return expectedFingerprints.Any(expected => expected == normalizedMd5 || expected == normalizedSha256);
    }

    private static bool KnownHostMatches(TransferettoSshKnownHostEntry entry, TransferettoSshHostKeyInfo hostKeyInfo) {
        if (!string.Equals(entry.HostKeyName, hostKeyInfo.HostKeyName, StringComparison.Ordinal)) {
            return false;
        }

        return FingerprintMatches(
            new[] {
                NormalizeFingerprint(entry.FingerPrintMD5),
                NormalizeFingerprint(entry.FingerPrintSHA256)
            }.Where(static value => !string.IsNullOrWhiteSpace(value)),
            hostKeyInfo);
    }

    private static string ResolveKnownHostsPath(TransferettoSshConnectionOptions options) {
        if (!string.IsNullOrWhiteSpace(options.KnownHostsPath)) {
            return options.KnownHostsPath!;
        }

        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root)) {
            root = AppDomain.CurrentDomain.BaseDirectory;
        }

        return Path.Combine(root, "Transferetto", "ssh-known-hosts.tsv");
    }

    private static List<TransferettoSshKnownHostEntry> LoadKnownHosts(string path) {
        if (!File.Exists(path)) {
            return new List<TransferettoSshKnownHostEntry>();
        }

        List<TransferettoSshKnownHostEntry> entries = new();
        foreach (string rawLine in File.ReadAllLines(path)) {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) {
                continue;
            }

            string[] parts = line.Split('\t');
            if (parts.Length < 8) {
                continue;
            }

            if (!int.TryParse(parts[1], out int port)) {
                continue;
            }

            if (!int.TryParse(parts[5], out int keyLength)) {
                keyLength = 0;
            }

            entries.Add(new TransferettoSshKnownHostEntry {
                Host = parts[0],
                Port = port,
                HostKeyName = parts[2],
                FingerPrintMD5 = parts[3],
                FingerPrintSHA256 = parts[4],
                KeyLength = keyLength,
                FirstSeenUtc = parts[6],
                LastSeenUtc = parts[7]
            });
        }

        return entries;
    }

    private static void SaveKnownHosts(string path, IEnumerable<TransferettoSshKnownHostEntry> entries) {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        string[] lines = entries.Select(SerializeKnownHostEntry).ToArray();
        File.WriteAllLines(path, lines);
    }

    private static string SerializeKnownHostEntry(TransferettoSshKnownHostEntry entry) {
        return string.Join("\t", new[] {
            SanitizeKnownHostValue(entry.Host),
            entry.Port.ToString(),
            SanitizeKnownHostValue(entry.HostKeyName),
            SanitizeKnownHostValue(entry.FingerPrintMD5),
            SanitizeKnownHostValue(entry.FingerPrintSHA256),
            entry.KeyLength.ToString(),
            SanitizeKnownHostValue(entry.FirstSeenUtc),
            SanitizeKnownHostValue(entry.LastSeenUtc)
        });
    }

    private static string SanitizeKnownHostValue(string? value) {
        return (value ?? string.Empty).Replace("\t", " ").Trim();
    }

    private static TransferettoSshKnownHostEntry CreateKnownHostEntry(TransferettoSshConnectionOptions options, TransferettoSshHostKeyInfo hostKeyInfo) {
        string now = DateTime.UtcNow.ToString("O");
        return new TransferettoSshKnownHostEntry {
            Host = options.Server,
            Port = options.Port ?? 22,
            HostKeyName = hostKeyInfo.HostKeyName,
            FingerPrintMD5 = hostKeyInfo.FingerPrintMD5,
            FingerPrintSHA256 = hostKeyInfo.FingerPrintSHA256,
            KeyLength = hostKeyInfo.KeyLength,
            FirstSeenUtc = now,
            LastSeenUtc = now
        };
    }

    private static string ApplyLookback(string value, int lookback) {
        if (lookback <= 0 || value.Length <= lookback) {
            return value;
        }

        return value.Substring(value.Length - lookback, lookback);
    }

    private static string TrimLeadingCommandEcho(string output, string command) {
        if (string.IsNullOrEmpty(output)) {
            return output;
        }

        string normalizedCommand = command.Trim();
        string trimmedOutput = output.TrimStart('\r', '\n');
        if (trimmedOutput.StartsWith(normalizedCommand, StringComparison.Ordinal)) {
            string remaining = trimmedOutput.Substring(normalizedCommand.Length);
            return remaining.TrimStart('\r', '\n');
        }

        return output;
    }
}
