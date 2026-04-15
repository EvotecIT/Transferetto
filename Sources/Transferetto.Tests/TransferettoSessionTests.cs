using System.Reflection;
using System.Net;
using FluentFTP;
using Renci.SshNet;
using Transferetto;

namespace Transferetto.Tests;

public sealed class TransferettoSessionTests {
    [Fact]
    public void FtpSessionExposesWrappedClientProperties() {
        FtpClient client = new("ftp.example.com");
        TransferettoFtpSession session = CreateSession(client, "none");

        Assert.Equal("ftp.example.com", session.Host);
        Assert.False(session.IsConnected);
        Assert.Equal("none", session.Error);
    }

    [Fact]
    public void RuntimeSettingsStoresTraceOptions() {
        TransferettoFtpTraceOptions traceOptions = new() {
            LogToConsole = true,
            LogHost = true,
            LogPassword = false,
            LogUserName = false
        };

        TransferettoRuntimeSettings.FtpTraceOptions = traceOptions;

        Assert.Same(traceOptions, TransferettoRuntimeSettings.FtpTraceOptions);
    }

    [Fact]
    public void SshShellOptionsProvideInteractiveDefaults() {
        TransferettoSshShellOptions options = new() {
            TerminalName = "xterm",
            Columns = 120,
            Rows = 50,
            Width = 0,
            Height = 0,
            BufferSize = 2048,
            PromptPattern = "(?m)^[$#]\\s?$"
        };

        Assert.Equal("xterm", options.TerminalName);
        Assert.Equal((uint) 120, options.Columns);
        Assert.Equal((uint) 50, options.Rows);
        Assert.Equal(2048, options.BufferSize);
        Assert.False(options.NoTerminal);
        Assert.Equal("(?m)^[$#]\\s?$", options.PromptPattern);
        Assert.True(options.EnableTranscript);
        Assert.Equal(500, options.MaxTranscriptEntries);
        Assert.Equal(262144, options.MaxTranscriptCharacters);
    }

    [Fact]
    public void SshTunnelSessionExposesConfiguredProperties() {
        TransferettoSshSession sshSession = CreateSshSession(new SshClient("ssh.example.com", "user", "password"), null);
        using ForwardedPortLocal forwardedPort = new("127.0.0.1", 15432, "127.0.0.1", 5432);
        TransferettoSshTunnelSession tunnelSession = CreateTunnelSession(sshSession, forwardedPort, TransferettoSshTunnelType.Local, "127.0.0.1", 15432, "127.0.0.1", 5432);

        Assert.Equal(TransferettoSshTunnelType.Local, tunnelSession.TunnelType);
        Assert.Equal("127.0.0.1", tunnelSession.BoundHost);
        Assert.Equal((uint) 15432, tunnelSession.BoundPort);
        Assert.Equal("127.0.0.1", tunnelSession.Host);
        Assert.Equal((uint) 5432, tunnelSession.Port);
    }

    [Fact]
    public void SshConnectionOptionsStoreAuthAndPolicySettings() {
        NetworkCredential credential = new("user", "password");
        NetworkCredential proxyCredential = new("proxyUser", "proxyPassword");
        TransferettoSshConnectionOptions options = new() {
            Server = "ssh.example.com",
            UserName = "user",
            Password = "password",
            Credential = credential,
            PrivateKeyPath = "C:\\keys\\id_rsa",
            PrivateKeyPassphrase = "secret",
            Port = 2222,
            EnableKeyboardInteractive = true,
            AcceptAnyHostKey = false,
            ExpectedHostKeyFingerprints = new[] { "SHA256:abc123=", "MD5:AA:BB:CC" },
            HostKeyPolicy = TransferettoSshHostKeyPolicy.KnownHosts,
            KnownHostsPath = "C:\\temp\\ssh-known-hosts.tsv",
            KeepAliveIntervalSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            RetryAttempts = 2,
            ProxyType = TransferettoSshProxyType.Socks5,
            ProxyHost = "proxy.example.com",
            ProxyPort = 1080,
            ProxyCredential = proxyCredential
        };

        Assert.Equal("ssh.example.com", options.Server);
        Assert.Equal("user", options.UserName);
        Assert.Equal("password", options.Password);
        Assert.Same(credential, options.Credential);
        Assert.Equal("C:\\keys\\id_rsa", options.PrivateKeyPath);
        Assert.Equal("secret", options.PrivateKeyPassphrase);
        Assert.Equal(2222, options.Port);
        Assert.True(options.EnableKeyboardInteractive);
        Assert.False(options.AcceptAnyHostKey);
        Assert.Equal(2, options.ExpectedHostKeyFingerprints!.Length);
        Assert.Equal(TransferettoSshHostKeyPolicy.KnownHosts, options.HostKeyPolicy);
        Assert.Equal("C:\\temp\\ssh-known-hosts.tsv", options.KnownHostsPath);
        Assert.Equal(30, options.KeepAliveIntervalSeconds);
        Assert.Equal(15, options.ConnectionTimeoutSeconds);
        Assert.Equal(2, options.RetryAttempts);
        Assert.Equal(TransferettoSshProxyType.Socks5, options.ProxyType);
        Assert.Equal("proxy.example.com", options.ProxyHost);
        Assert.Equal(1080, options.ProxyPort);
        Assert.Same(proxyCredential, options.ProxyCredential);
    }

    [Fact]
    public void SshSessionExposesHostKeyInfo() {
        TransferettoSshSession session = CreateSshSession(new SshClient("ssh.example.com", "user", "password"), null);
        TransferettoSshHostKeyInfo hostKeyInfo = new() {
            HostKeyName = "ssh-ed25519",
            KeyLength = 256,
            FingerPrintMD5 = "aa:bb:cc",
            FingerPrintSHA256 = "SHA256:abc123=",
            CanTrust = true,
            TrustSource = TransferettoSshHostKeyTrustSource.KnownHosts,
            KnownHostsPath = "C:\\temp\\ssh-known-hosts.tsv",
            WasPersisted = false
        };

        PropertyInfo hostKeyInfoProperty = typeof(TransferettoSshSession).GetProperty(nameof(TransferettoSshSession.HostKeyInfo))!;
        hostKeyInfoProperty.SetValue(session, hostKeyInfo);

        Assert.NotNull(session.HostKeyInfo);
        Assert.Equal("ssh-ed25519", session.HostKeyInfo!.HostKeyName);
        Assert.True(session.HostKeyInfo.CanTrust);
        Assert.Equal(TransferettoSshHostKeyTrustSource.KnownHosts, session.HostKeyInfo.TrustSource);
        Assert.Equal("C:\\temp\\ssh-known-hosts.tsv", session.HostKeyInfo.KnownHostsPath);
    }

    [Fact]
    public void SshConnectionOptionsDefaultToTrustOnFirstUse() {
        TransferettoSshConnectionOptions options = new();

        Assert.Equal(TransferettoSshHostKeyPolicy.TrustOnFirstUse, options.HostKeyPolicy);
        Assert.Null(options.KnownHostsPath);
    }

    [Fact]
    public void ScpSessionExposesWrappedClientProperties() {
        TransferettoScpSession session = CreateScpSession(new ScpClient("scp.example.com", "user", "password"), "none");
        TransferettoSshHostKeyInfo hostKeyInfo = new() {
            HostKeyName = "ssh-rsa",
            KeyLength = 2048,
            FingerPrintMD5 = "aa:bb:cc",
            FingerPrintSHA256 = "SHA256:def456=",
            CanTrust = true,
            TrustSource = TransferettoSshHostKeyTrustSource.ExpectedFingerprint
        };

        PropertyInfo hostKeyInfoProperty = typeof(TransferettoScpSession).GetProperty(nameof(TransferettoScpSession.HostKeyInfo))!;
        hostKeyInfoProperty.SetValue(session, hostKeyInfo);

        Assert.Equal("scp.example.com", session.Host);
        Assert.Equal(22, session.Port);
        Assert.False(session.IsConnected);
        Assert.Equal("none", session.Error);
        Assert.NotNull(session.HostKeyInfo);
        Assert.Equal("ssh-rsa", session.HostKeyInfo!.HostKeyName);
        Assert.Equal(TransferettoSshHostKeyTrustSource.ExpectedFingerprint, session.HostKeyInfo.TrustSource);
    }

    [Fact]
    public void SftpStreamResultsExposeChunkMetadata() {
        TransferettoSftpStreamReadResult readResult = new() {
            Status = true,
            Path = "/pub/example/demo.txt",
            Data = new byte[] { 1, 2, 3 },
            BytesRead = 3,
            Position = 3,
            EndOfStream = false
        };
        TransferettoSftpStreamWriteResult writeResult = new() {
            Status = true,
            Path = "/pub/example/demo.txt",
            BytesWritten = 12,
            Position = 12
        };

        Assert.Equal("ReadStream", readResult.Action);
        Assert.Equal(3, readResult.BytesRead);
        Assert.False(readResult.EndOfStream);
        Assert.Equal("WriteStream", writeResult.Action);
        Assert.Equal(12, writeResult.BytesWritten);
        Assert.Equal(12, writeResult.Position);
    }

    [Fact]
    public void SftpStreamModeProvidesReadWriteAppendOptions() {
        string[] names = Enum.GetNames(typeof(TransferettoSftpStreamMode));

        Assert.Equal(new[] { "Read", "Write", "Append" }, names);
    }

    [Fact]
    public void FtpStreamResultsExposeChunkMetadata() {
        TransferettoFtpStreamReadResult readResult = new() {
            Status = true,
            Path = "/pub/example/demo.txt",
            Data = new byte[] { 4, 5, 6 },
            BytesRead = 3,
            Position = 3,
            EndOfStream = false
        };
        TransferettoFtpStreamWriteResult writeResult = new() {
            Status = true,
            Path = "/pub/example/demo.txt",
            BytesWritten = 24,
            Position = 24
        };

        Assert.Equal("ReadStream", readResult.Action);
        Assert.Equal(3, readResult.BytesRead);
        Assert.False(readResult.EndOfStream);
        Assert.Equal("WriteStream", writeResult.Action);
        Assert.Equal(24, writeResult.BytesWritten);
        Assert.Equal(24, writeResult.Position);
    }

    [Fact]
    public void FtpStreamModeProvidesReadWriteAppendOptions() {
        string[] names = Enum.GetNames(typeof(TransferettoFtpStreamMode));

        Assert.Equal(new[] { "Read", "Write", "Append" }, names);
    }

    [Fact]
    public void SftpPathHelpersNormalizeAndCombineRemotePaths() {
        MethodInfo normalizeMethod = typeof(TransferettoClient).GetMethod("NormalizeRemotePath", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo combineMethod = typeof(TransferettoClient).GetMethod("CombineRemotePath", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo parentMethod = typeof(TransferettoClient).GetMethod("GetRemoteParent", BindingFlags.Static | BindingFlags.NonPublic)!;

        string normalized = (string) normalizeMethod.Invoke(null, new object[] { @"\pub\example\" })!;
        string combined = (string) combineMethod.Invoke(null, new object[] { "/pub/example", "child/file.txt" })!;
        string parent = (string) parentMethod.Invoke(null, new object[] { "/pub/example/child" })!;

        Assert.Equal("/pub/example", normalized);
        Assert.Equal("/pub/example/child/file.txt", combined);
        Assert.Equal("/pub/example", parent);
    }

    [Fact]
    public void SshShellCommandResultExposesParsedState() {
        TransferettoSshShellCommandResult result = new() {
            Command = "pwd",
            Output = "/var/www",
            ExitCode = 0,
            Status = true,
            Marker = "__TRANSFERETTO__123",
            PromptPattern = "(?m)^[$#]\\s?$"
        };

        Assert.Equal("pwd", result.Command);
        Assert.Equal("/var/www", result.Output);
        Assert.Equal(0, result.ExitCode);
        Assert.True(result.Status);
        Assert.Equal("__TRANSFERETTO__123", result.Marker);
        Assert.Equal("(?m)^[$#]\\s?$", result.PromptPattern);
    }

    [Fact]
    public void SshShellControlKeyProvidesInteractiveActions() {
        string[] names = Enum.GetNames(typeof(TransferettoSshShellControlKey));

        Assert.Equal(new[] { "Interrupt", "EndOfTransmission", "Suspend", "EndOfText", "Escape", "Tab", "Enter" }, names);
    }

    [Fact]
    public void SshShellControlTextMapsToExpectedCharacters() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("GetSshShellControlText", BindingFlags.Static | BindingFlags.NonPublic)!;

        string interrupt = (string) method.Invoke(null, new object[] { TransferettoSshShellControlKey.Interrupt })!;
        string escape = (string) method.Invoke(null, new object[] { TransferettoSshShellControlKey.Escape })!;
        string enter = (string) method.Invoke(null, new object[] { TransferettoSshShellControlKey.Enter })!;

        Assert.Equal("\u0003", interrupt);
        Assert.Equal("\u001B", escape);
        Assert.Equal("\r", enter);
    }

    [Fact]
    public void SshShellTranscriptSnapshotExposesStructuredEntries() {
        TransferettoSshShellTranscriptSnapshot snapshot = new() {
            Entries = new[] {
                new TransferettoSshShellTranscriptEntry {
                    TimestampUtc = new DateTime(2026, 4, 15, 12, 30, 0, DateTimeKind.Utc),
                    Direction = TransferettoSshShellTranscriptDirection.Write,
                    Text = "ls -la"
                },
                new TransferettoSshShellTranscriptEntry {
                    TimestampUtc = new DateTime(2026, 4, 15, 12, 30, 1, DateTimeKind.Utc),
                    Direction = TransferettoSshShellTranscriptDirection.Read,
                    Text = "total 12"
                }
            },
            CapturedEntryCount = 2,
            DroppedEntryCount = 3,
            TotalCharacterCount = 14
        };

        Assert.Equal(2, snapshot.CapturedEntryCount);
        Assert.Equal(3, snapshot.DroppedEntryCount);
        Assert.True(snapshot.IsTruncated);
        Assert.Equal(TransferettoSshShellTranscriptDirection.Write, snapshot.Entries[0].Direction);
        Assert.Equal("total 12", snapshot.Entries[1].Text);
    }

    [Fact]
    public void SshShellTranscriptFormattingIncludesDirectionAndDropSummary() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("FormatSshShellTranscript", BindingFlags.Static | BindingFlags.NonPublic)!;
        TransferettoSshShellTranscriptSnapshot snapshot = new() {
            Entries = new[] {
                new TransferettoSshShellTranscriptEntry {
                    TimestampUtc = new DateTime(2026, 4, 15, 12, 30, 0, DateTimeKind.Utc),
                    Direction = TransferettoSshShellTranscriptDirection.Control,
                    Text = "<Interrupt>"
                }
            },
            CapturedEntryCount = 1,
            DroppedEntryCount = 2,
            TotalCharacterCount = 11
        };

        string text = (string) method.Invoke(null, new object[] { snapshot })!;

        Assert.Contains("[Control]", text);
        Assert.Contains("<Interrupt>", text);
        Assert.Contains("Dropped 2 older entries", text);
    }

    [Fact]
    public void SftpAttributesExposeCalculatedPermissions() {
        TransferettoSftpAttributes attributes = new() {
            Path = "/var/www/index.html",
            PermissionsValue = Convert.ToInt16("644", 8),
            PermissionsOctal = "644",
            OwnerCanRead = true,
            OwnerCanWrite = true,
            OwnerCanExecute = false,
            GroupCanRead = true,
            GroupCanWrite = false,
            GroupCanExecute = false,
            OthersCanRead = true,
            OthersCanWrite = false,
            OthersCanExecute = false
        };

        Assert.Equal("/var/www/index.html", attributes.Path);
        Assert.Equal("644", attributes.PermissionsOctal);
        Assert.Equal(Convert.ToInt16("644", 8), attributes.PermissionsValue);
        Assert.True(attributes.OwnerCanWrite);
        Assert.False(attributes.GroupCanWrite);
    }

    [Fact]
    public void SftpPermissionParserAcceptsOctalStrings() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("ParseSftpPermissions", BindingFlags.Static | BindingFlags.NonPublic)!;

        short mode644 = (short) method.Invoke(null, new object[] { "644" })!;
        short mode755 = (short) method.Invoke(null, new object[] { "0755" })!;

        Assert.Equal(Convert.ToInt16("644", 8), mode644);
        Assert.Equal(Convert.ToInt16("755", 8), mode755);
    }

    [Fact]
    public void SftpItemExposesSymbolicLinkState() {
        TransferettoSftpItem item = new() {
            Name = "current",
            FullName = "/var/www/current",
            IsDirectory = false,
            IsRegularFile = false,
            IsSymbolicLink = true,
            Length = 0
        };

        Assert.Equal("current", item.Name);
        Assert.True(item.IsSymbolicLink);
        Assert.False(item.IsDirectory);
        Assert.False(item.IsRegularFile);
    }

    [Fact]
    public void FtpRemoteItemExposesMetadataFields() {
        TransferettoRemoteItem item = new() {
            Name = "index.html",
            FullName = "/public_html/index.html",
            Type = FluentFTP.FtpObjectType.File,
            Modified = new DateTime(2026, 4, 15, 12, 0, 0, DateTimeKind.Utc),
            Created = new DateTime(2026, 4, 15, 11, 0, 0, DateTimeKind.Utc),
            Size = 12345,
            LinkTarget = null,
            RawPermissions = "-rw-r--r--"
        };

        Assert.Equal("index.html", item.Name);
        Assert.Equal("/public_html/index.html", item.FullName);
        Assert.Equal(FluentFTP.FtpObjectType.File, item.Type);
        Assert.Equal(12345, item.Size);
        Assert.Equal("-rw-r--r--", item.RawPermissions);
    }

    private static TransferettoFtpSession CreateSession(FtpClient client, string? error) {
        ConstructorInfo constructor = typeof(TransferettoFtpSession).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoFtpSession) constructor.Invoke(new object?[] { client, null, error });
    }

    private static TransferettoSshSession CreateSshSession(SshClient client, string? error) {
        ConstructorInfo constructor = typeof(TransferettoSshSession).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoSshSession) constructor.Invoke(new object?[] { client, error });
    }

    private static TransferettoScpSession CreateScpSession(ScpClient client, string? error) {
        ConstructorInfo constructor = typeof(TransferettoScpSession).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoScpSession) constructor.Invoke(new object?[] { client, error });
    }

    private static TransferettoSshTunnelSession CreateTunnelSession(
        TransferettoSshSession sshSession,
        ForwardedPort forwardedPort,
        TransferettoSshTunnelType tunnelType,
        string boundHost,
        uint boundPort,
        string host,
        uint port) {
        ConstructorInfo constructor = typeof(TransferettoSshTunnelSession).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoSshTunnelSession) constructor.Invoke(new object?[] { sshSession, forwardedPort, tunnelType, boundHost, boundPort, host, port });
    }
}
