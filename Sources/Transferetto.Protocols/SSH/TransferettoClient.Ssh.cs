using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        return SendSshCommand(session, commands, null);
    }
    /// <summary>
    /// Runs one or more non-interactive SSH commands with shared execution options.
    /// </summary>

    public static TransferettoSshCommandResult SendSshCommand(
        TransferettoSshSession session,
        IEnumerable<string> commands,
        TransferettoSshCommandOptions? options) {
        return SendSshCommandAsync(session, commands, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Runs one or more non-interactive SSH commands asynchronously.
    /// </summary>

    public static Task<TransferettoSshCommandResult> SendSshCommandAsync(
        TransferettoSshSession session,
        IEnumerable<string> commands,
        TransferettoSshCommandOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNull(commands, nameof(commands));

        return ExecuteSshCommandAsync(session, BuildSshCommandText(commands), options, cancellationToken);
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
    /// Resolves an SSH shell prompt pattern from an explicit pattern or a reusable preset.
    /// </summary>

    public static string? ResolveSshShellPromptPattern(
        string? promptPattern,
        TransferettoSshShellPromptPreset promptPreset = TransferettoSshShellPromptPreset.None) {
        if (!string.IsNullOrWhiteSpace(promptPattern)) {
            return promptPattern;
        }

        return promptPreset switch {
            TransferettoSshShellPromptPreset.None => null,
            TransferettoSshShellPromptPreset.Linux => @"(?m)^[^\r\n]*[#$]\s?$",
            TransferettoSshShellPromptPreset.LinuxUser => @"(?m)^[^\r\n]*\$\s?$",
            TransferettoSshShellPromptPreset.LinuxRoot => @"(?m)^[^\r\n]*#\s?$",
            TransferettoSshShellPromptPreset.PowerShell => @"(?m)^PS [^\r\n]*>\s?$",
            TransferettoSshShellPromptPreset.Cmd => @"(?m)^[A-Za-z]:\\[^\r\n>]*>\s?$",
            _ => throw new ArgumentOutOfRangeException(nameof(promptPreset), promptPreset, "Unsupported SSH shell prompt preset.")
        };
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
        return ReadSshShell(session, timeout, readLine, expectText, lookback, regexPattern, readUntilIdle, idleTimeout, expectPrompt, promptPattern, null);
    }
    /// <summary>
    /// Reads text from an interactive SSH shell session with shared read options.
    /// </summary>

    public static string ReadSshShell(
        TransferettoSshShellSession session,
        TimeSpan? timeout,
        bool readLine,
        string? expectText,
        int lookback,
        string? regexPattern,
        bool readUntilIdle,
        TimeSpan? idleTimeout,
        bool expectPrompt,
        string? promptPattern,
        TransferettoSshShellReadOptions? options) {
        return ReadSshShellAsync(session, timeout, readLine, expectText, lookback, regexPattern, readUntilIdle, idleTimeout, expectPrompt, promptPattern, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Reads text from an interactive SSH shell session asynchronously.
    /// </summary>

    public static Task<string> ReadSshShellAsync(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        bool readLine = false,
        string? expectText = null,
        int lookback = -1,
        string? regexPattern = null,
        bool readUntilIdle = false,
        TimeSpan? idleTimeout = null,
        bool expectPrompt = false,
        string? promptPattern = null,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));

        if (!string.IsNullOrWhiteSpace(regexPattern)) {
            return ReadSshShellRegexAsync(session, regexPattern!, timeout, lookback, options, cancellationToken);
        }

        if (expectPrompt || !string.IsNullOrWhiteSpace(promptPattern)) {
            return WaitForSshShellPromptAsync(session, timeout, promptPattern, lookback, options, cancellationToken);
        }

        if (readUntilIdle) {
            return ReadSshShellUntilIdleAsync(session, idleTimeout ?? TimeSpan.FromMilliseconds(500), timeout, options, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(expectText)) {
            return ReadSshShellUntilTextAsync(session, expectText!, timeout, lookback, options, cancellationToken);
        }

        if (readLine) {
            return ReadSshShellLineAsync(session, timeout, options, cancellationToken);
        }

        if (timeout.HasValue) {
            return ReadSshShellWithTimeoutAsync(session, timeout.Value, options, cancellationToken);
        }

        return Task.FromResult(ReadAvailableSshShellOutput(session, options));
    }
    /// <summary>
    /// Reads SSH shell output until a regular expression matches.
    /// </summary>

    public static string ReadSshShellRegex(
        TransferettoSshShellSession session,
        string pattern,
        TimeSpan? timeout = null,
        int lookback = -1) {
        return ReadSshShellRegex(session, pattern, timeout, lookback, null);
    }
    /// <summary>
    /// Reads SSH shell output until a regular expression matches.
    /// </summary>

    public static string ReadSshShellRegex(
        TransferettoSshShellSession session,
        string pattern,
        TimeSpan? timeout,
        int lookback,
        TransferettoSshShellReadOptions? options) {
        return ReadSshShellRegexAsync(session, pattern, timeout, lookback, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Reads SSH shell output until a regular expression matches asynchronously.
    /// </summary>

    public static Task<string> ReadSshShellRegexAsync(
        TransferettoSshShellSession session,
        string pattern,
        TimeSpan? timeout = null,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(pattern, nameof(pattern));

        Regex regex = new(pattern, RegexOptions.Multiline);
        return ReadSshShellUntilRegexMatchAsync(session, regex, timeout, lookback, options, cancellationToken);
    }
    /// <summary>
    /// Reads SSH shell output until it becomes idle.
    /// </summary>

    public static string ReadSshShellUntilIdle(
        TransferettoSshShellSession session,
        TimeSpan idleTimeout,
        TimeSpan? timeout = null) {
        return ReadSshShellUntilIdle(session, idleTimeout, timeout, null);
    }
    /// <summary>
    /// Reads SSH shell output until it becomes idle.
    /// </summary>

    public static string ReadSshShellUntilIdle(
        TransferettoSshShellSession session,
        TimeSpan idleTimeout,
        TimeSpan? timeout,
        TransferettoSshShellReadOptions? options) {
        return ReadSshShellUntilIdleAsync(session, idleTimeout, timeout, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Reads SSH shell output until it becomes idle asynchronously.
    /// </summary>

    public static async Task<string> ReadSshShellUntilIdleAsync(
        TransferettoSshShellSession session,
        TimeSpan idleTimeout,
        TimeSpan? timeout = null,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));

        if (idleTimeout <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(idleTimeout), "Idle timeout must be greater than zero.");
        }

        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch overallStopwatch = Stopwatch.StartNew();
        Stopwatch idleStopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();
        bool receivedAnyData = false;

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            if (TryAppendAvailableSshShellOutput(session, builder, options)) {
                receivedAnyData = true;
                idleStopwatch.Restart();
                continue;
            }

            if (receivedAnyData && idleStopwatch.Elapsed >= idleTimeout) {
                return builder.ToString();
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && overallStopwatch.Elapsed >= timeout.Value) {
                return builder.ToString();
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Writes text to an interactive SSH shell session.
    /// </summary>

    public static void WriteSshShell(TransferettoSshShellSession session, string? text, bool appendLine = true) {
        WriteSshShell(session, text, appendLine, recordTranscript: true);
    }

    private static void WriteSshShell(TransferettoSshShellSession session, string? text, bool appendLine, bool recordTranscript) {
        EnsureNotNull(session, nameof(session));

        string content = text ?? string.Empty;
        if (appendLine) {
            session.ShellStream.WriteLine(content);
            if (recordTranscript) {
                session.RecordTranscript(TransferettoSshShellTranscriptDirection.Write, content + Environment.NewLine);
            }
        } else {
            session.ShellStream.Write(content);
            if (recordTranscript) {
                session.RecordTranscript(TransferettoSshShellTranscriptDirection.Write, content);
            }
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

        string pendingOutput = session.ConsumePendingReadOutput();
        StringBuilder freshOutputBuilder = new();
        while (session.ShellStream.DataAvailable) {
            freshOutputBuilder.Append(session.ShellStream.Read());
        }

        string freshOutput = freshOutputBuilder.ToString();
        session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, freshOutput);
        return pendingOutput + freshOutput;
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
        return StopSshShellCommand(session, timeout, promptPattern, lookback, null);
    }
    /// <summary>
    /// Stops a running SSH shell command and collects trailing output.
    /// </summary>

    public static string StopSshShellCommand(
        TransferettoSshShellSession session,
        TimeSpan? timeout,
        string? promptPattern,
        int lookback,
        TransferettoSshShellReadOptions? options) {
        return StopSshShellCommandAsync(session, timeout, promptPattern, lookback, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Stops a running SSH shell command and collects trailing output asynchronously.
    /// </summary>

    public static async Task<string> StopSshShellCommandAsync(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));

        SendSshShellControl(session, TransferettoSshShellControlKey.Interrupt);

        string? resolvedPromptPattern = !string.IsNullOrWhiteSpace(promptPattern)
            ? promptPattern
            : session.PromptPattern;

        if (!string.IsNullOrWhiteSpace(resolvedPromptPattern)) {
            return await WaitForSshShellPromptAsync(
                session,
                timeout ?? TimeSpan.FromSeconds(5),
                resolvedPromptPattern,
                lookback,
                options,
                cancellationToken).ConfigureAwait(false);
        }

        return await ReadSshShellUntilIdleAsync(
            session,
            TimeSpan.FromMilliseconds(500),
            timeout ?? TimeSpan.FromSeconds(5),
            options,
            cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Updates the expected prompt pattern for an SSH shell session.
    /// </summary>

    public static void SetSshShellPromptPattern(TransferettoSshShellSession session, string? promptPattern) {
        SetSshShellPromptPattern(session, promptPattern, TransferettoSshShellPromptPreset.None);
    }
    /// <summary>
    /// Updates the expected prompt pattern for an SSH shell session.
    /// </summary>

    public static void SetSshShellPromptPattern(
        TransferettoSshShellSession session,
        string? promptPattern,
        TransferettoSshShellPromptPreset promptPreset) {
        EnsureNotNull(session, nameof(session));
        session.UpdatePromptPattern(ResolveSshShellPromptPattern(promptPattern, promptPreset), promptPreset);
    }
    /// <summary>
    /// Waits for an SSH shell prompt to appear.
    /// </summary>

    public static string WaitForSshShellPrompt(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        int lookback = -1) {
        return WaitForSshShellPrompt(session, timeout, promptPattern, lookback, null);
    }
    /// <summary>
    /// Waits for an SSH shell prompt to appear.
    /// </summary>

    public static string WaitForSshShellPrompt(
        TransferettoSshShellSession session,
        TimeSpan? timeout,
        string? promptPattern,
        int lookback,
        TransferettoSshShellReadOptions? options) {
        return WaitForSshShellPromptAsync(session, timeout, promptPattern, lookback, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Waits for an SSH shell prompt to appear asynchronously.
    /// </summary>

    public static Task<string> WaitForSshShellPromptAsync(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));

        string? resolvedPattern = !string.IsNullOrWhiteSpace(promptPattern)
            ? promptPattern
            : session.PromptPattern;

        if (string.IsNullOrWhiteSpace(resolvedPattern)) {
            throw new InvalidOperationException("No SSH shell prompt pattern was configured.");
        }

        Regex regex = new(resolvedPattern, RegexOptions.Multiline);
        return ReadSshShellUntilRegexMatchAsync(session, regex, timeout, lookback, options, cancellationToken);
    }
    /// <summary>
    /// Follows SSH shell output until cancellation, timeout, or an optional stop pattern is observed.
    /// </summary>

    public static string FollowSshShellOutput(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? stopPattern = null,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null) {
        return FollowSshShellOutputAsync(session, timeout, stopPattern, lookback, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Follows SSH shell output until cancellation, timeout, or an optional stop pattern is observed.
    /// </summary>

    public static async Task<string> FollowSshShellOutputAsync(
        TransferettoSshShellSession session,
        TimeSpan? timeout = null,
        string? stopPattern = null,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));

        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch stopwatch = Stopwatch.StartNew();
        Regex? stopRegex = !string.IsNullOrWhiteSpace(stopPattern) ? new Regex(stopPattern, RegexOptions.Multiline) : null;
        StringBuilder builder = new();

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            if (TryAppendAvailableSshShellOutput(session, builder, options)) {
                if (stopRegex is null) {
                    continue;
                }

                string current = builder.ToString();
                string search = ApplyLookback(current, lookback);
                if (stopRegex.IsMatch(search)) {
                    return current;
                }

                continue;
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && stopwatch.Elapsed >= timeout.Value) {
                return builder.ToString();
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Executes a reusable SSH shell recipe against an interactive shell session.
    /// </summary>

    public static TransferettoSshShellRecipeResult InvokeSshShellRecipe(
        TransferettoSshShellSession session,
        TransferettoSshShellRecipeOptions recipe,
        TransferettoSshShellReadOptions? options = null) {
        return InvokeSshShellRecipeAsync(session, recipe, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Executes a reusable SSH shell recipe against an interactive shell session asynchronously.
    /// </summary>

    public static Task<TransferettoSshShellRecipeResult> InvokeSshShellRecipeAsync(
        TransferettoSshShellSession session,
        TransferettoSshShellRecipeOptions recipe,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNull(recipe, nameof(recipe));

        return recipe.Recipe switch {
            TransferettoSshShellRecipeKind.SudoCommand => InvokeSshShellSudoRecipeAsync(session, recipe, options, cancellationToken),
            TransferettoSshShellRecipeKind.FollowFile => InvokeSshShellFollowRecipeAsync(session, recipe, BuildSshShellFollowFileCommand(recipe.RemotePath!, recipe.TailLines), options, cancellationToken),
            TransferettoSshShellRecipeKind.FollowJournal => InvokeSshShellFollowRecipeAsync(session, recipe, BuildSshShellFollowJournalCommand(recipe.ServiceName!, recipe.TailLines), options, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(recipe), recipe.Recipe, "Unsupported SSH shell recipe.")
        };
    }
    /// <summary>
    /// Executes an ordered expect workflow against an interactive SSH shell session.
    /// </summary>

    public static TransferettoSshShellExpectResult InvokeSshShellExpect(
        TransferettoSshShellSession session,
        IEnumerable<TransferettoSshShellExpectStep> steps,
        TransferettoSshShellReadOptions? options = null) {
        return InvokeSshShellExpectAsync(session, steps, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Executes an ordered expect workflow against an interactive SSH shell session asynchronously.
    /// </summary>

    public static async Task<TransferettoSshShellExpectResult> InvokeSshShellExpectAsync(
        TransferettoSshShellSession session,
        IEnumerable<TransferettoSshShellExpectStep> steps,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNull(steps, nameof(steps));

        TransferettoSshShellExpectStep[] resolvedSteps = steps
            .Where(static step => step is not null)
            .ToArray();
        if (resolvedSteps.Length == 0) {
            throw new InvalidOperationException("At least one SSH shell expect step must be provided.");
        }

        List<TransferettoSshShellExpectStepResult> results = new(resolvedSteps.Length);
        for (int i = 0; i < resolvedSteps.Length; i++) {
            TransferettoSshShellExpectStep step = resolvedSteps[i];
            DateTime startedUtc = DateTime.UtcNow;

            if (step.ControlKey.HasValue) {
                SendSshShellControl(session, step.ControlKey.Value, step.ControlRepeat > 0 ? step.ControlRepeat : 1);
            }

            if (!string.IsNullOrEmpty(step.SendText)) {
                WriteSshShell(session, step.SendText, step.AppendLine);
            }

            string output = string.Empty;
            if (StepRequiresRead(step)) {
                output = await ExecuteSshShellExpectReadAsync(session, step, options, cancellationToken).ConfigureAwait(false);
            }

            bool matched = EvaluateSshShellExpectStep(step, session, output);
            bool timedOut = StepHasExplicitExpectation(step) && !matched && step.Timeout.HasValue && step.Timeout.Value >= TimeSpan.Zero;
            TransferettoSshShellExpectStepResult result = new() {
                StepIndex = i,
                StepName = step.Name,
                Status = matched,
                Matched = matched,
                TimedOut = timedOut,
                Output = output,
                StartedUtc = startedUtc,
                CompletedUtc = DateTime.UtcNow
            };
            results.Add(result);

            if (!matched) {
                return new TransferettoSshShellExpectResult {
                    Status = false,
                    FailedStepIndex = i,
                    CompletedStepCount = results.Count,
                    Steps = results.ToArray()
                };
            }
        }

        return new TransferettoSshShellExpectResult {
            Status = true,
            CompletedStepCount = results.Count,
            Steps = results.ToArray()
        };
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
        return InvokeSshShellCommand(session, command, timeout, promptPattern, trimCommandEcho, lookback, null);
    }
    /// <summary>
    /// Runs a command through an interactive SSH shell and captures the result.
    /// </summary>

    public static TransferettoSshShellCommandResult InvokeSshShellCommand(
        TransferettoSshShellSession session,
        string command,
        TimeSpan? timeout,
        string? promptPattern,
        bool trimCommandEcho,
        int lookback,
        TransferettoSshShellReadOptions? options) {
        return InvokeSshShellCommandAsync(session, command, timeout, promptPattern, trimCommandEcho, lookback, options).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Runs a command through an interactive SSH shell and captures the result asynchronously.
    /// </summary>

    public static async Task<TransferettoSshShellCommandResult> InvokeSshShellCommandAsync(
        TransferettoSshShellSession session,
        string command,
        TimeSpan? timeout = null,
        string? promptPattern = null,
        bool trimCommandEcho = true,
        int lookback = -1,
        TransferettoSshShellReadOptions? options = null,
        CancellationToken cancellationToken = default) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(command, nameof(command));

        string marker = "__TRANSFERETTO__" + Guid.NewGuid().ToString("N");
        WriteSshShell(session, command, appendLine: true);
        WriteSshShell(session, $"printf '{marker}:%s\\n' $?", appendLine: true);

        Regex markerRegex = new($"(?m)^{Regex.Escape(marker)}:(-?\\d+)\\r?$", RegexOptions.Multiline);
        string output = await ReadSshShellUntilRegexMatchAsync(session, markerRegex, timeout, lookback, options, cancellationToken).ConfigureAwait(false);
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
                string trailingOutput = await WaitForSshShellPromptAsync(
                    session,
                    timeout ?? TimeSpan.FromSeconds(5),
                    resolvedPromptPattern,
                    lookback,
                    options,
                    cancellationToken).ConfigureAwait(false);
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
            Status = IsSshShellCommandSuccessful(exitCode),
            Marker = marker,
            PromptPattern = resolvedPromptPattern
        };
    }
    /// <summary>
    /// Executes a reusable SSH shell recipe that runs a sudo command through the interactive shell.
    /// </summary>

    private static async Task<TransferettoSshShellRecipeResult> InvokeSshShellSudoRecipeAsync(
        TransferettoSshShellSession session,
        TransferettoSshShellRecipeOptions recipe,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        EnsureNotNullOrWhiteSpace(recipe.Command, nameof(recipe.Command));

        DateTime startedUtc = DateTime.UtcNow;
        string marker = "__TRANSFERETTO__" + Guid.NewGuid().ToString("N");
        string passwordPromptToken = "__TRANSFERETTO_SUDO__" + Guid.NewGuid().ToString("N");
        string commandText = BuildSshShellSudoCommand(recipe.Command!, marker, passwordPromptToken);
        string? resolvedPromptPattern = ResolveSshShellRecipePromptPattern(recipe, session);
        Regex markerRegex = new($"(?m)^{Regex.Escape(marker)}:(-?\\d+)\\r?$", RegexOptions.Multiline);
        string passwordPromptPattern = !string.IsNullOrWhiteSpace(recipe.PasswordPromptPattern)
            ? recipe.PasswordPromptPattern!
            : BuildDefaultSudoPasswordPromptPattern(passwordPromptToken);
        Regex initialRegex = new($"(?m)({passwordPromptPattern})|^{Regex.Escape(marker)}:(-?\\d+)\\r?$", RegexOptions.Multiline);

        WriteSshShell(session, commandText, appendLine: true);
        string output = await ReadSshShellUntilRegexMatchAsync(session, initialRegex, recipe.Timeout, recipe.Lookback, options, cancellationToken).ConfigureAwait(false);

        if (!markerRegex.IsMatch(ApplyLookback(output, recipe.Lookback))) {
            if (string.IsNullOrEmpty(recipe.Password)) {
                throw new InvalidOperationException("Sudo recipe requires Password when a sudo password prompt is shown.");
            }

            WriteSshShell(session, recipe.Password, appendLine: true, recordTranscript: false);
            Regex completionRegex = new($"(?m)({passwordPromptPattern})|^{Regex.Escape(marker)}:(-?\\d+)\\r?$", RegexOptions.Multiline);
            string trailingOutput = await ReadSshShellUntilRegexMatchAsync(session, completionRegex, recipe.Timeout, recipe.Lookback, options, cancellationToken).ConfigureAwait(false);
            output += trailingOutput;

            if (!markerRegex.IsMatch(ApplyLookback(output, recipe.Lookback))) {
                throw new InvalidOperationException("Sudo password prompt was shown again before the command completed.");
            }
        }

        Match markerMatch = markerRegex.Match(output);
        int? exitCode = null;
        if (markerMatch.Success && int.TryParse(markerMatch.Groups[1].Value, out int parsedExitCode)) {
            exitCode = parsedExitCode;
        }

        string cleanedOutput = markerRegex.Replace(output, string.Empty);
        cleanedOutput = Regex.Replace(cleanedOutput, passwordPromptPattern, string.Empty, RegexOptions.Multiline).TrimEnd();
        cleanedOutput = TrimLeadingCommandEcho(cleanedOutput, commandText);
        cleanedOutput = await AppendTrailingPromptOutputAsync(session, cleanedOutput, output, markerMatch, resolvedPromptPattern, recipe.Timeout, recipe.Lookback, options, cancellationToken).ConfigureAwait(false);

        return new TransferettoSshShellRecipeResult {
            Recipe = recipe.Recipe,
            CommandText = commandText,
            Output = cleanedOutput,
            ExitCode = exitCode,
            Status = IsSshShellCommandSuccessful(exitCode),
            PromptPattern = resolvedPromptPattern,
            StartedUtc = startedUtc,
            CompletedUtc = DateTime.UtcNow
        };
    }

    private static async Task<TransferettoSshShellRecipeResult> InvokeSshShellFollowRecipeAsync(
        TransferettoSshShellSession session,
        TransferettoSshShellRecipeOptions recipe,
        string commandText,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        DateTime startedUtc = DateTime.UtcNow;
        string? resolvedPromptPattern = ResolveSshShellRecipePromptPattern(recipe, session);
        string output = string.Empty;
        string trailingOutput = string.Empty;
        bool wasInterrupted = false;

        WriteSshShell(session, commandText, appendLine: true);
        try {
            output = await FollowSshShellOutputAsync(session, recipe.Timeout, recipe.StopPattern, recipe.Lookback, options, cancellationToken).ConfigureAwait(false);
        } finally {
            try {
                trailingOutput = await StopSshShellCommandAsync(
                    session,
                    recipe.InterruptTimeout ?? TimeSpan.FromSeconds(5),
                    resolvedPromptPattern,
                    recipe.Lookback,
                    CreateCleanupSshShellReadOptions(options),
                    CancellationToken.None).ConfigureAwait(false);
                wasInterrupted = true;
            } catch when (session.IsConnected) {
                // Best-effort cleanup: preserve the original recipe failure or cancellation.
            }
        }

        string combinedOutput = string.IsNullOrWhiteSpace(trailingOutput)
            ? output.TrimEnd()
            : string.IsNullOrWhiteSpace(output)
                ? trailingOutput.TrimEnd()
                : (output.TrimEnd() + Environment.NewLine + trailingOutput.TrimEnd()).TrimEnd();

        return new TransferettoSshShellRecipeResult {
            Recipe = recipe.Recipe,
            CommandText = commandText,
            Output = combinedOutput,
            Status = true,
            WasInterrupted = wasInterrupted,
            PromptPattern = resolvedPromptPattern,
            StartedUtc = startedUtc,
            CompletedUtc = DateTime.UtcNow
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
        bool hasProxyHost = !string.IsNullOrWhiteSpace(options.ProxyHost);
        bool hasProxyPort = options.ProxyPort.HasValue && options.ProxyPort.Value > 0;
        bool hasProxyCredential = options.ProxyCredential is not null;

        if (options.ProxyType == TransferettoSshProxyType.None) {
            if (hasProxyHost || hasProxyPort || hasProxyCredential) {
                throw new InvalidOperationException("ProxyType must be specified when ProxyHost, ProxyPort, or ProxyCredential is provided.");
            }

            return;
        }

        if (!hasProxyHost) {
            throw new InvalidOperationException("ProxyHost must be provided when ProxyType is enabled.");
        }

        if (!hasProxyPort) {
            throw new InvalidOperationException("ProxyPort must be a positive value when ProxyType is enabled.");
        }
    }

    private static bool IsSshShellCommandSuccessful(int? exitCode) {
        return exitCode.HasValue && exitCode.Value == 0;
    }

    private static string? ResolveSshShellRecipePromptPattern(
        TransferettoSshShellRecipeOptions recipe,
        TransferettoSshShellSession session) {
        return ResolveSshShellPromptPattern(recipe.PromptPattern, recipe.PromptPreset) ?? session.PromptPattern;
    }

    private static async Task<string> AppendTrailingPromptOutputAsync(
        TransferettoSshShellSession session,
        string cleanedOutput,
        string rawOutput,
        Match markerMatch,
        string? promptPattern,
        TimeSpan? timeout,
        int lookback,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(promptPattern)) {
            return cleanedOutput;
        }

        Regex promptRegex = new(promptPattern, RegexOptions.Multiline);
        string trailingAfterMarker = markerMatch.Success
            ? rawOutput.Substring(markerMatch.Index + markerMatch.Length)
            : string.Empty;

        if (promptRegex.IsMatch(trailingAfterMarker)) {
            return cleanedOutput;
        }

        string trailingOutput = await WaitForSshShellPromptAsync(
            session,
            timeout ?? TimeSpan.FromSeconds(5),
            promptPattern,
            lookback,
            options,
            cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(trailingOutput)) {
            return cleanedOutput;
        }

        return string.IsNullOrEmpty(cleanedOutput)
            ? trailingOutput.TrimEnd()
            : (cleanedOutput + Environment.NewLine + trailingOutput.TrimEnd()).TrimEnd();
    }

    private static async Task<string> ExecuteSshShellExpectReadAsync(
        TransferettoSshShellSession session,
        TransferettoSshShellExpectStep step,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        if (step.Follow) {
            string? stopPattern = ResolveSshShellExpectStopPattern(step, session);
            return await FollowSshShellOutputAsync(session, step.Timeout, stopPattern, step.Lookback, options, cancellationToken).ConfigureAwait(false);
        }

        string? promptPattern = ResolveSshShellPromptPattern(step.PromptPattern, step.PromptPreset);
        if (step.ExpectPrompt || !string.IsNullOrWhiteSpace(promptPattern)) {
            return await WaitForSshShellPromptAsync(session, step.Timeout, promptPattern, step.Lookback, options, cancellationToken).ConfigureAwait(false);
        }

        return await ReadSshShellAsync(
            session,
            step.Timeout,
            step.ReadLine,
            step.ExpectText,
            step.Lookback,
            step.RegexPattern,
            step.ReadUntilIdle,
            step.IdleTimeout,
            expectPrompt: false,
            promptPattern: null,
            options,
            cancellationToken).ConfigureAwait(false);
    }

    private static bool EvaluateSshShellExpectStep(
        TransferettoSshShellExpectStep step,
        TransferettoSshShellSession session,
        string output) {
        if (!StepHasExplicitExpectation(step)) {
            return true;
        }

        string search = ApplyLookback(output, step.Lookback);
        if (step.Follow) {
            string? stopPattern = ResolveSshShellExpectStopPattern(step, session);
            return string.IsNullOrWhiteSpace(stopPattern) || new Regex(stopPattern, RegexOptions.Multiline).IsMatch(search);
        }

        string? promptPattern = ResolveSshShellPromptPattern(step.PromptPattern, step.PromptPreset) ?? session.PromptPattern;
        if (step.ExpectPrompt || !string.IsNullOrWhiteSpace(promptPattern)) {
            return !string.IsNullOrWhiteSpace(promptPattern) && new Regex(promptPattern, RegexOptions.Multiline).IsMatch(search);
        }

        if (!string.IsNullOrWhiteSpace(step.RegexPattern)) {
            return new Regex(step.RegexPattern, RegexOptions.Multiline).IsMatch(search);
        }

        if (!string.IsNullOrWhiteSpace(step.ExpectText)) {
            return search.IndexOf(step.ExpectText, StringComparison.Ordinal) >= 0;
        }

        return true;
    }

    private static bool StepRequiresRead(TransferettoSshShellExpectStep step) {
        return step.ReadLine ||
            step.ReadUntilIdle ||
            step.Follow ||
            step.ExpectPrompt ||
            !string.IsNullOrWhiteSpace(step.PromptPattern) ||
            step.PromptPreset != TransferettoSshShellPromptPreset.None ||
            !string.IsNullOrWhiteSpace(step.ExpectText) ||
            !string.IsNullOrWhiteSpace(step.RegexPattern) ||
            !string.IsNullOrWhiteSpace(step.StopPattern);
    }

    private static bool StepHasExplicitExpectation(TransferettoSshShellExpectStep step) {
        return step.Follow ||
            step.ExpectPrompt ||
            !string.IsNullOrWhiteSpace(step.PromptPattern) ||
            step.PromptPreset != TransferettoSshShellPromptPreset.None ||
            !string.IsNullOrWhiteSpace(step.ExpectText) ||
            !string.IsNullOrWhiteSpace(step.RegexPattern) ||
            !string.IsNullOrWhiteSpace(step.StopPattern);
    }

    private static string? ResolveSshShellExpectStopPattern(TransferettoSshShellExpectStep step, TransferettoSshShellSession session) {
        if (!string.IsNullOrWhiteSpace(step.StopPattern)) {
            return step.StopPattern;
        }

        if (!string.IsNullOrWhiteSpace(step.RegexPattern)) {
            return step.RegexPattern;
        }

        if (!string.IsNullOrWhiteSpace(step.ExpectText)) {
            return Regex.Escape(step.ExpectText);
        }

        string? promptPattern = ResolveSshShellPromptPattern(step.PromptPattern, step.PromptPreset);
        if (step.ExpectPrompt || !string.IsNullOrWhiteSpace(promptPattern)) {
            return !string.IsNullOrWhiteSpace(promptPattern)
                ? promptPattern
                : session.PromptPattern;
        }

        return null;
    }

    private static async Task<TransferettoSshCommandResult> ExecuteSshCommandAsync(
        TransferettoSshSession session,
        string commandText,
        TransferettoSshCommandOptions? options,
        CancellationToken cancellationToken) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(commandText, nameof(commandText));

        CancellationTokenSource? linkedCancellationSource = null;
        DateTime startedUtc = DateTime.UtcNow;

        using SshCommand command = session.Client.CreateCommand(commandText);
        try {
            TransferettoSshCommandOptions? resolvedOptions = ResolveAsyncSshCommandOptions(options, cancellationToken, out linkedCancellationSource);
            if (resolvedOptions?.CommandTimeout is TimeSpan timeout) {
                command.CommandTimeout = timeout;
            }

            CancellationToken effectiveCancellationToken = resolvedOptions?.CancellationToken ?? cancellationToken;
            Encoding outputEncoding = resolvedOptions?.OutputEncoding ?? Encoding.UTF8;
            StringBuilder stdout = new();
            StringBuilder stderr = new();

            Task executeTask = command.ExecuteAsync(effectiveCancellationToken);
            Task stdoutTask = ReadSshCommandStreamAsync(command.OutputStream, TransferettoSshCommandOutputStream.Stdout, stdout, outputEncoding, resolvedOptions?.OutputProgress, effectiveCancellationToken);
            Task stderrTask = ReadSshCommandStreamAsync(command.ExtendedOutputStream, TransferettoSshCommandOutputStream.Stderr, stderr, outputEncoding, resolvedOptions?.OutputProgress, effectiveCancellationToken);

            bool isCanceled = false;
            try {
                await executeTask.ConfigureAwait(false);
            } catch (OperationCanceledException) when (effectiveCancellationToken.IsCancellationRequested) {
                isCanceled = true;
                TryCancelSshCommand(command);
            } finally {
                await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
            }

            DateTime completedUtc = DateTime.UtcNow;
            return new TransferettoSshCommandResult {
                CommandText = commandText,
                Status = !isCanceled && command.ExitStatus == 0,
                ExitStatus = command.ExitStatus,
                ExitSignal = string.IsNullOrWhiteSpace(command.ExitSignal) ? null : command.ExitSignal,
                IsCanceled = isCanceled,
                Output = stdout.ToString(),
                Error = stderr.Length > 0 ? stderr.ToString() : null,
                StartedUtc = startedUtc,
                CompletedUtc = completedUtc
            };
        } finally {
            linkedCancellationSource?.Dispose();
        }
    }

    private static async Task ReadSshCommandStreamAsync(
        Stream stream,
        TransferettoSshCommandOutputStream outputStream,
        StringBuilder builder,
        Encoding encoding,
        IProgress<TransferettoSshCommandOutputChunk>? progress,
        CancellationToken cancellationToken) {
        using StreamReader reader = new(stream, encoding, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
        CancellationTokenRegistration cancellationRegistration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(static state => DisposeSshCommandOutputStream((Stream) state!), stream)
            : default;
        char[] buffer = new char[4096];
        try {
            while (true) {
                int readCount;
                try {
                    readCount = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                } catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested) {
                    break;
                } catch (IOException) when (cancellationToken.IsCancellationRequested) {
                    break;
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    break;
                }

                if (readCount == 0) {
                    break;
                }

                string text = new(buffer, 0, readCount);
                builder.Append(text);
                progress?.Report(new TransferettoSshCommandOutputChunk {
                    Stream = outputStream,
                    Text = text,
                    TimestampUtc = DateTime.UtcNow
                });
            }
        } finally {
            cancellationRegistration.Dispose();
        }
    }

    private static void DisposeSshCommandOutputStream(Stream stream) {
        try {
            stream.Dispose();
        } catch (ObjectDisposedException) {
        }
    }

    private static TransferettoSshCommandOptions? ResolveAsyncSshCommandOptions(
        TransferettoSshCommandOptions? options,
        CancellationToken cancellationToken,
        out CancellationTokenSource? linkedCancellationSource) {
        linkedCancellationSource = null;

        if (!cancellationToken.CanBeCanceled) {
            return options;
        }

        if (options is null) {
            return new TransferettoSshCommandOptions {
                CancellationToken = cancellationToken
            };
        }

        if (!options.CancellationToken.CanBeCanceled) {
            return CloneSshCommandOptions(options, cancellationToken);
        }

        linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken, cancellationToken);
        return CloneSshCommandOptions(options, linkedCancellationSource.Token);
    }

    private static TransferettoSshCommandOptions CloneSshCommandOptions(TransferettoSshCommandOptions options, CancellationToken cancellationToken) {
        return new TransferettoSshCommandOptions {
            CancellationToken = cancellationToken,
            CommandTimeout = options.CommandTimeout,
            OutputEncoding = options.OutputEncoding,
            OutputProgress = options.OutputProgress
        };
    }

    private static TransferettoSshShellReadOptions? CreateCleanupSshShellReadOptions(TransferettoSshShellReadOptions? options) {
        if (options is null) {
            return null;
        }

        return new TransferettoSshShellReadOptions {
            PollInterval = options.PollInterval,
            OutputProgress = options.OutputProgress
        };
    }

    private static string BuildSshCommandText(IEnumerable<string> commands) {
        string commandText = string.Join(string.Empty, commands
            .Where(static command => !string.IsNullOrWhiteSpace(command))
            .Select(static command => command.TrimEnd().EndsWith(";") ? command : $"{command};"));

        if (string.IsNullOrWhiteSpace(commandText)) {
            throw new InvalidOperationException("At least one SSH command must be provided.");
        }

        return commandText;
    }

    private static string BuildSshShellSudoCommand(string command, string marker, string passwordPromptToken) {
        EnsureNotNullOrWhiteSpace(command, nameof(command));
        EnsureNotNullOrWhiteSpace(marker, nameof(marker));
        EnsureNotNullOrWhiteSpace(passwordPromptToken, nameof(passwordPromptToken));

        string commandWithMarker = $"{command}; printf '{marker}:%s\\n' $?";
        return $"sudo -S -p {WrapSingleQuotedShellText(passwordPromptToken)} sh -lc {WrapSingleQuotedShellText(commandWithMarker)}";
    }

    private static string BuildSshShellFollowFileCommand(string remotePath, int tailLines) {
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        if (tailLines <= 0) {
            throw new ArgumentOutOfRangeException(nameof(tailLines), "Tail lines must be greater than zero.");
        }

        return $"tail -n {tailLines} -f -- {WrapSingleQuotedShellText(remotePath)}";
    }

    private static string BuildSshShellFollowJournalCommand(string serviceName, int tailLines) {
        EnsureNotNullOrWhiteSpace(serviceName, nameof(serviceName));

        if (tailLines <= 0) {
            throw new ArgumentOutOfRangeException(nameof(tailLines), "Tail lines must be greater than zero.");
        }

        return $"journalctl -u {WrapSingleQuotedShellText(serviceName)} -n {tailLines} -f --no-pager";
    }

    private static string WrapSingleQuotedShellText(string value) {
        return "'" + EscapeSingleQuotedShellText(value) + "'";
    }

    private static string EscapeSingleQuotedShellText(string value) {
        return value.Replace("'", "'\"'\"'");
    }

    private static void TryCancelSshCommand(SshCommand command) {
        try {
            command.CancelAsync(forceKill: false, millisecondsTimeout: 1000);
        } catch {
            // Best-effort cancellation: preserve the original cancellation flow.
        }
    }

    private static async Task<string> ReadSshShellWithTimeoutAsync(
        TransferettoSshShellSession session,
        TimeSpan timeout,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan) {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative or Timeout.InfiniteTimeSpan.");
        }

        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            string output = ReadAvailableSshShellOutput(session, options);
            if (!string.IsNullOrEmpty(output)) {
                return output;
            }

            if (timeout != Timeout.InfiniteTimeSpan && stopwatch.Elapsed >= timeout) {
                return string.Empty;
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadSshShellUntilRegexMatchAsync(
        TransferettoSshShellSession session,
        Regex regex,
        TimeSpan? timeout,
        int lookback,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            if (TryAppendAvailableSshShellOutput(session, builder, options)) {
                string current = builder.ToString();
                string search = ApplyLookback(current, lookback);
                if (regex.IsMatch(search)) {
                    return current;
                }

                continue;
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && stopwatch.Elapsed >= timeout.Value) {
                return builder.ToString();
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadSshShellUntilTextAsync(
        TransferettoSshShellSession session,
        string expectedText,
        TimeSpan? timeout,
        int lookback,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            if (TryAppendAvailableSshShellOutput(session, builder, options)) {
                string current = builder.ToString();
                string search = ApplyLookback(current, lookback);
                if (search.IndexOf(expectedText, StringComparison.Ordinal) >= 0) {
                    return current;
                }

                continue;
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && stopwatch.Elapsed >= timeout.Value) {
                return builder.ToString();
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadSshShellLineAsync(
        TransferettoSshShellSession session,
        TimeSpan? timeout,
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken) {
        using CancellationTokenSource? linkedCancellationSource = CreateLinkedCancellationTokenSource(options is null ? default : options.CancellationToken, cancellationToken);
        CancellationToken effectiveCancellationToken = ResolveSshShellReadCancellationToken(options, cancellationToken, linkedCancellationSource);
        TimeSpan pollInterval = ResolveShellPollInterval(options);
        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();

        while (true) {
            effectiveCancellationToken.ThrowIfCancellationRequested();

            if (TryAppendAvailableSshShellOutput(session, builder, options)) {
                string current = builder.ToString();
                (string line, string remainder) = SplitCompletedSshLine(current);
                if (line.Length > 0) {
                    if (remainder.Length > 0) {
                        session.AppendPendingReadOutput(remainder);
                    }

                    return line;
                }

                continue;
            }

            if (timeout.HasValue && timeout.Value >= TimeSpan.Zero && stopwatch.Elapsed >= timeout.Value) {
                return builder.ToString();
            }

            await Task.Delay(pollInterval, effectiveCancellationToken).ConfigureAwait(false);
        }
    }

    private static string ReadAvailableSshShellOutput(TransferettoSshShellSession session, TransferettoSshShellReadOptions? options) {
        string pendingOutput = session.ConsumePendingReadOutput();
        if (!string.IsNullOrEmpty(pendingOutput)) {
            return pendingOutput;
        }

        if (!session.ShellStream.DataAvailable) {
            return string.Empty;
        }

        string output = session.ShellStream.Read();
        ReportSshShellChunk(session, output, options);
        return output;
    }

    private static bool TryAppendAvailableSshShellOutput(
        TransferettoSshShellSession session,
        StringBuilder builder,
        TransferettoSshShellReadOptions? options) {
        string output = ReadAvailableSshShellOutput(session, options);
        if (string.IsNullOrEmpty(output)) {
            return false;
        }

        builder.Append(output);
        return true;
    }

    private static void ReportSshShellChunk(
        TransferettoSshShellSession session,
        string? text,
        TransferettoSshShellReadOptions? options) {
        if (text is not string chunkText || chunkText.Length == 0) {
            return;
        }

        session.RecordTranscript(TransferettoSshShellTranscriptDirection.Read, chunkText);
        options?.OutputProgress?.Report(new TransferettoSshShellOutputChunk {
            Text = chunkText,
            TimestampUtc = DateTime.UtcNow
        });
    }

    private static CancellationTokenSource? CreateLinkedCancellationTokenSource(CancellationToken primary, CancellationToken secondary) {
        if (primary.CanBeCanceled && secondary.CanBeCanceled) {
            return CancellationTokenSource.CreateLinkedTokenSource(primary, secondary);
        }

        return null;
    }

    private static TimeSpan ResolveShellPollInterval(TransferettoSshShellReadOptions? options) {
        return options?.PollInterval > TimeSpan.Zero
            ? options.PollInterval
            : TimeSpan.FromMilliseconds(50);
    }

    private static CancellationToken ResolveSshShellReadCancellationToken(
        TransferettoSshShellReadOptions? options,
        CancellationToken cancellationToken,
        CancellationTokenSource? linkedCancellationSource) {
        if (linkedCancellationSource is not null) {
            return linkedCancellationSource.Token;
        }

        if (options is not null && options.CancellationToken.CanBeCanceled) {
            return options.CancellationToken;
        }

        return cancellationToken;
    }

    private static string BuildDefaultSudoPasswordPromptPattern(string passwordPromptToken) {
        EnsureNotNullOrWhiteSpace(passwordPromptToken, nameof(passwordPromptToken));
        return $"(?:^|[\\r\\n]){Regex.Escape(passwordPromptToken)}\\s*$";
    }

    private static (string Line, string Remainder) SplitCompletedSshLine(string text) {
        if (string.IsNullOrEmpty(text)) {
            return (string.Empty, string.Empty);
        }

        int newlineIndex = text.IndexOf('\n');
        if (newlineIndex < 0) {
            return (string.Empty, text);
        }

        return (text.Substring(0, newlineIndex + 1), text.Substring(newlineIndex + 1));
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
