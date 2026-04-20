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
    public void FtpConnectionOptionsStoreCertificatePinningSettings() {
        TransferettoFtpConnectionOptions options = new() {
            Server = "ftps.example.com",
            ExpectedCertificateThumbprints = new[] { "SHA256:ABCDEF", "aa:bb:cc" },
            ValidateAnyCertificate = false,
            CertificatePolicy = TransferettoFtpCertificatePolicy.KnownCertificates,
            KnownCertificatesPath = "C:\\temp\\ftps-known-certificates.tsv"
        };

        Assert.Equal("ftps.example.com", options.Server);
        Assert.Equal(2, options.ExpectedCertificateThumbprints!.Length);
        Assert.False(options.ValidateAnyCertificate);
        Assert.Equal(TransferettoFtpCertificatePolicy.KnownCertificates, options.CertificatePolicy);
        Assert.Equal("C:\\temp\\ftps-known-certificates.tsv", options.KnownCertificatesPath);
    }

    [Fact]
    public void FtpConnectionOptionsStoreAdvancedRuntimeSettings() {
        TransferettoFtpConnectionOptions options = new() {
            Server = "ftp.example.com",
            UseGnuTls = true,
            ConnectTimeout = 15000,
            ReadTimeout = 16000,
            DataConnectionConnectTimeout = 17000,
            DataConnectionReadTimeout = 18000,
            NoopInterval = 0,
            SslSessionLength = 32,
            EncryptAuthenticationOnly = true,
            SelfConnectMode = FtpSelfConnectMode.Always,
            RetryAttempts = 3,
            TransferChunkSize = 65536,
            LocalFileBufferSize = 32768,
            InternetProtocolVersions = FtpIpVersion.ANY,
            UploadRateLimit = 512,
            DownloadRateLimit = 1024,
            UploadDataType = FtpDataType.Binary,
            DownloadDataType = FtpDataType.ASCII,
            ListingDataType = FtpDataType.ASCII,
            FXPDataType = FtpDataType.Binary,
            FXPProgressInterval = 250,
            ActivePorts = new[] { 50000, 50001 },
            PassiveBlockedPorts = new[] { 60000, 60001 },
            PassiveMaxAttempts = 4,
            EncodingName = "utf-8"
        };

        Assert.Equal(15000, options.ConnectTimeout);
        Assert.True(options.UseGnuTls);
        Assert.Equal(16000, options.ReadTimeout);
        Assert.Equal(17000, options.DataConnectionConnectTimeout);
        Assert.Equal(18000, options.DataConnectionReadTimeout);
        Assert.Equal(0, options.NoopInterval);
        Assert.Equal(32, options.SslSessionLength);
        Assert.True(options.EncryptAuthenticationOnly);
        Assert.Equal(FtpSelfConnectMode.Always, options.SelfConnectMode);
        Assert.Equal(3, options.RetryAttempts);
        Assert.Equal(65536, options.TransferChunkSize);
        Assert.Equal(32768, options.LocalFileBufferSize);
        Assert.Equal(FtpIpVersion.ANY, options.InternetProtocolVersions);
        Assert.Equal((uint) 512, options.UploadRateLimit);
        Assert.Equal((uint) 1024, options.DownloadRateLimit);
        Assert.Equal(FtpDataType.Binary, options.UploadDataType);
        Assert.Equal(FtpDataType.ASCII, options.DownloadDataType);
        Assert.Equal(FtpDataType.ASCII, options.ListingDataType);
        Assert.Equal(FtpDataType.Binary, options.FXPDataType);
        Assert.Equal(250, options.FXPProgressInterval);
        Assert.Equal(new[] { 50000, 50001 }, options.ActivePorts);
        Assert.Equal(new[] { 60000, 60001 }, options.PassiveBlockedPorts);
        Assert.Equal(4, options.PassiveMaxAttempts);
        Assert.Equal("utf-8", options.EncodingName);
    }

    [Fact]
    public void ConfigureFtpClientAppliesAdvancedRuntimeSettings() {
        using FtpClient client = new("ftp.example.com");
        TransferettoFtpConnectionOptions options = new() {
            EncryptionMode = new[] { FtpEncryptionMode.Explicit },
            UseGnuTls = true,
            ConnectTimeout = 15000,
            ReadTimeout = 16000,
            DataConnectionConnectTimeout = 17000,
            DataConnectionReadTimeout = 18000,
            NoopInterval = 0,
            SslSessionLength = 32,
            EncryptAuthenticationOnly = true,
            SelfConnectMode = FtpSelfConnectMode.Always,
            RetryAttempts = 3,
            TransferChunkSize = 65536,
            LocalFileBufferSize = 32768,
            InternetProtocolVersions = FtpIpVersion.ANY,
            UploadRateLimit = 512,
            DownloadRateLimit = 1024,
            UploadDataType = FtpDataType.Binary,
            DownloadDataType = FtpDataType.ASCII,
            ListingDataType = FtpDataType.ASCII,
            FXPDataType = FtpDataType.Binary,
            FXPProgressInterval = 250,
            ActivePorts = new[] { 50000, 50001 },
            PassiveBlockedPorts = new[] { 60000, 60001 },
            PassiveMaxAttempts = 4,
            EncodingName = "utf-8"
        };
        MethodInfo method = typeof(TransferettoClient).GetMethod("ConfigureFtpClient", BindingFlags.Static | BindingFlags.NonPublic)!;

        method.Invoke(null, new object[] { client, options });

        Assert.Equal(15000, client.Config.ConnectTimeout);
        Assert.Equal(16000, client.Config.ReadTimeout);
        Assert.Equal(17000, client.Config.DataConnectionConnectTimeout);
        Assert.Equal(18000, client.Config.DataConnectionReadTimeout);
        Assert.False(client.Config.Noop);
        Assert.Equal(32, client.Config.SslSessionLength);
        Assert.True(client.Config.EncryptAuthenticationOnly);
        Assert.Equal(FtpSelfConnectMode.Always, client.Config.SelfConnectMode);
        Assert.Equal(3, client.Config.RetryAttempts);
        Assert.Equal(65536, client.Config.TransferChunkSize);
        Assert.Equal(32768, client.Config.LocalFileBufferSize);
        Assert.Equal(FtpIpVersion.ANY, client.Config.InternetProtocolVersions);
        Assert.Equal((uint) 512, client.Config.UploadRateLimit);
        Assert.Equal((uint) 1024, client.Config.DownloadRateLimit);
        Assert.Equal(FtpDataType.Binary, client.Config.UploadDataType);
        Assert.Equal(FtpDataType.ASCII, client.Config.DownloadDataType);
        Assert.Equal(FtpDataType.ASCII, client.Config.ListingDataType);
        Assert.Equal(FtpDataType.Binary, client.Config.FXPDataType);
        Assert.Equal(250, client.Config.FXPProgressInterval);
        Assert.Equal(new[] { 50000, 50001 }, client.Config.ActivePorts);
        Assert.Equal(new[] { 60000, 60001 }, client.Config.PassiveBlockedPorts);
        Assert.Equal(4, client.Config.PassiveMaxAttempts);
        Assert.Equal("utf-8", client.Encoding.WebName);
        Assert.Equal("FluentFTP.GnuTLS.GnuTlsStream", client.Config.CustomStream?.FullName);
    }

    [Fact]
    public void FtpSessionExposesCertificateInfo() {
        TransferettoFtpSession session = CreateSession(new FtpClient("ftps.example.com"), null);
        TransferettoFtpCertificateInfo certificateInfo = new() {
            Subject = "CN=ftps.example.com",
            Issuer = "CN=Example CA",
            ThumbprintSHA1 = "SHA1:001122",
            ThumbprintSHA256 = "SHA256:AABBCC",
            CanTrust = true,
            TrustSource = TransferettoFtpCertificateTrustSource.ExpectedThumbprint,
            KnownCertificatesPath = "C:\\temp\\ftps-known-certificates.tsv",
            WasPersisted = true
        };

        PropertyInfo certificateInfoProperty = typeof(TransferettoFtpSession).GetProperty(nameof(TransferettoFtpSession.CertificateInfo))!;
        certificateInfoProperty.SetValue(session, certificateInfo);

        Assert.NotNull(session.CertificateInfo);
        Assert.True(session.CertificateInfo!.CanTrust);
        Assert.Equal("CN=ftps.example.com", session.CertificateInfo.Subject);
        Assert.Equal(TransferettoFtpCertificateTrustSource.ExpectedThumbprint, session.CertificateInfo.TrustSource);
        Assert.Equal("C:\\temp\\ftps-known-certificates.tsv", session.CertificateInfo.KnownCertificatesPath);
        Assert.True(session.CertificateInfo.WasPersisted);
    }

    [Fact]
    public void FtpCertificateThumbprintsNormalizeForComparison() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("NormalizeCertificateThumbprint", BindingFlags.Static | BindingFlags.NonPublic)!;

        string sha1 = (string) method.Invoke(null, new object[] { "aa:bb cc-dd" })!;
        string sha256 = (string) method.Invoke(null, new object[] { "SHA256:aa:bb cc-dd" })!;

        Assert.Equal("AABBCCDD", sha1);
        Assert.Equal("SHA256:AABBCCDD", sha256);
    }

    [Fact]
    public void FtpChmodFallbackUsesStructuredPermissionFlags() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("ResolveFtpChmod", BindingFlags.Static | BindingFlags.NonPublic)!;
        FtpListItem item = new() {
            OwnerPermissions = FtpPermission.Read | FtpPermission.Write | FtpPermission.Execute,
            GroupPermissions = FtpPermission.Read | FtpPermission.Execute,
            OthersPermissions = FtpPermission.Read | FtpPermission.Execute
        };

        int chmod = (int) method.Invoke(null, new object?[] { item })!;

        Assert.Equal(755, chmod);
    }

    [Fact]
    public void FtpChmodFallbackUsesRawPermissionsWhenStructuredFlagsAreUnavailable() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("ResolveFtpChmod", BindingFlags.Static | BindingFlags.NonPublic)!;
        FtpListItem item = new() {
            RawPermissions = "-rw-r-----"
        };

        int chmod = (int) method.Invoke(null, new object?[] { item })!;

        Assert.Equal(640, chmod);
    }

    [Fact]
    public void FtpListingPathSplitterExtractsWildcardDirectoryAndPattern() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("TrySplitFtpListingPath", BindingFlags.Static | BindingFlags.NonPublic)!;
        object?[] arguments = { "/LOGS/Force_Data/0113*", null, null };

        bool result = (bool) method.Invoke(null, arguments)!;

        Assert.True(result);
        Assert.Equal("/LOGS/Force_Data", arguments[1]);
        Assert.Equal("0113*", arguments[2]);
    }

    [Fact]
    public void FxpPreflightReportsDisconnectedSessions() {
        TransferettoFtpSession source = CreateSession(new FtpClient("source.example.com"), null);
        TransferettoFtpSession destination = CreateSession(new FtpClient("destination.example.com"), null);

        TransferettoFxpPreflightResult result = TransferettoClient.TestFxpTransfer(
            source,
            "/exports/file.zip",
            destination,
            "/imports/file.zip",
            TransferettoFxpTransferKind.File,
            createRemoteDirectory: true);

        Assert.False(result.Status);
        Assert.False(result.SourceConnected);
        Assert.False(result.DestinationConnected);
        Assert.Equal(TransferettoFxpTransferKind.File, result.TransferKind);
        Assert.Contains(result.Messages, message => message.Contains("Source FTP session is not connected", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("Destination FTP session is not connected", StringComparison.Ordinal));
    }

    [Fact]
    public void FtpKnownCertificateTrustOnFirstUsePersistsAndReusesCertificate() {
        string path = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"), "ftps-known-certificates.tsv");
        TransferettoFtpConnectionOptions options = new() {
            Server = "ftps.example.com",
            Port = 990,
            KnownCertificatesPath = path,
            CertificatePolicy = TransferettoFtpCertificatePolicy.TrustOnFirstUse
        };
        TransferettoFtpCertificateInfo firstSeenCertificate = new() {
            Subject = "CN=ftps.example.com",
            Issuer = "CN=Example CA",
            ThumbprintSHA1 = "SHA1:0011223344556677889900112233445566778899",
            ThumbprintSHA256 = "SHA256:AABBCCDDEEFF0011223344556677889900112233445566778899001122334455"
        };
        MethodInfo method = typeof(TransferettoClient).GetMethod("EvaluateKnownCertificateTrust", BindingFlags.Static | BindingFlags.NonPublic)!;

        TransferettoFtpCertificateInfo trustedFirstSeen = (TransferettoFtpCertificateInfo) method.Invoke(null, new object[] { options, firstSeenCertificate, true })!;
        TransferettoFtpCertificateInfo knownCertificate = new() {
            Subject = firstSeenCertificate.Subject,
            Issuer = firstSeenCertificate.Issuer,
            ThumbprintSHA1 = firstSeenCertificate.ThumbprintSHA1,
            ThumbprintSHA256 = firstSeenCertificate.ThumbprintSHA256
        };
        TransferettoFtpCertificateInfo trustedKnownCertificate = (TransferettoFtpCertificateInfo) method.Invoke(null, new object[] { options, knownCertificate, false })!;

        Assert.True(trustedFirstSeen.CanTrust);
        Assert.True(trustedFirstSeen.WasPersisted);
        Assert.Equal(TransferettoFtpCertificateTrustSource.TrustOnFirstUse, trustedFirstSeen.TrustSource);
        Assert.True(File.Exists(path));
        Assert.True(trustedKnownCertificate.CanTrust);
        Assert.False(trustedKnownCertificate.WasPersisted);
        Assert.Equal(TransferettoFtpCertificateTrustSource.KnownCertificates, trustedKnownCertificate.TrustSource);
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
    public void ConnectSshRejectsConflictingTrustSettings() {
        TransferettoSshConnectionOptions options = new() {
            Server = "ssh.example.com",
            UserName = "user",
            Password = "password",
            AcceptAnyHostKey = true,
            ExpectedHostKeyFingerprints = new[] { "SHA256:abc123=" }
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectSsh(options));

        Assert.Contains("AcceptAnyHostKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectSftpRejectsConflictingTrustSettings() {
        TransferettoSftpConnectionOptions options = new() {
            Server = "sftp.example.com",
            UserName = "user",
            Password = "password",
            AcceptAnyHostKey = true,
            ExpectedHostKeyFingerprints = new[] { "SHA256:abc123=" }
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectSftp(options));

        Assert.Contains("AcceptAnyHostKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectScpRejectsConflictingTrustSettings() {
        TransferettoSshConnectionOptions options = new() {
            Server = "scp.example.com",
            UserName = "user",
            Password = "password",
            AcceptAnyHostKey = true,
            ExpectedHostKeyFingerprints = new[] { "SHA256:abc123=" }
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectScp(options));

        Assert.Contains("AcceptAnyHostKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectSshRejectsMissingProxyPortWhenProxyIsEnabled() {
        TransferettoSshConnectionOptions options = new() {
            Server = "ssh.example.com",
            UserName = "user",
            Password = "password",
            ProxyType = TransferettoSshProxyType.Socks5,
            ProxyHost = "proxy.example.com"
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectSsh(options));

        Assert.Contains("ProxyPort", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectSftpRejectsMissingProxyPortWhenProxyIsEnabled() {
        TransferettoSftpConnectionOptions options = new() {
            Server = "sftp.example.com",
            UserName = "user",
            Password = "password",
            ProxyType = TransferettoSshProxyType.Socks5,
            ProxyHost = "proxy.example.com"
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectSftp(options));

        Assert.Contains("ProxyPort", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectScpRejectsMissingProxyPortWhenProxyIsEnabled() {
        TransferettoSshConnectionOptions options = new() {
            Server = "scp.example.com",
            UserName = "user",
            Password = "password",
            ProxyType = TransferettoSshProxyType.Socks5,
            ProxyHost = "proxy.example.com"
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => TransferettoClient.ConnectScp(options));

        Assert.Contains("ProxyPort", exception.Message, StringComparison.Ordinal);
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
    public void SftpConnectionOptionsUseSshTrustAndProxySettings() {
        NetworkCredential credential = new("user", "password");
        NetworkCredential proxyCredential = new("proxyUser", "proxyPassword");
        TransferettoSftpConnectionOptions options = new() {
            Server = "sftp.example.com",
            UserName = "user",
            Password = "password",
            Credential = credential,
            PrivateKeyPath = "C:\\keys\\id_ed25519",
            PrivateKeyPassphrase = "secret",
            Port = 2222,
            EnableKeyboardInteractive = true,
            AcceptAnyHostKey = false,
            ExpectedHostKeyFingerprints = new[] { "SHA256:abc123=" },
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

        Assert.Equal("sftp.example.com", options.Server);
        Assert.Equal("user", options.UserName);
        Assert.Equal("password", options.Password);
        Assert.Same(credential, options.Credential);
        Assert.Equal("C:\\keys\\id_ed25519", options.PrivateKeyPath);
        Assert.Equal("secret", options.PrivateKeyPassphrase);
        Assert.Equal(2222, options.Port);
        Assert.True(options.EnableKeyboardInteractive);
        Assert.Equal("SHA256:abc123=", Assert.Single(options.ExpectedHostKeyFingerprints!));
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
    public void SftpSessionExposesHostKeyInfo() {
        TransferettoSftpSession session = CreateSftpSession(new SftpClient("sftp.example.com", "user", "password"), "none");
        TransferettoSshHostKeyInfo hostKeyInfo = new() {
            HostKeyName = "ssh-ed25519",
            KeyLength = 256,
            FingerPrintMD5 = "aa:bb:cc",
            FingerPrintSHA256 = "SHA256:def456=",
            CanTrust = true,
            TrustSource = TransferettoSshHostKeyTrustSource.KnownHosts,
            KnownHostsPath = "C:\\temp\\ssh-known-hosts.tsv"
        };

        PropertyInfo hostKeyInfoProperty = typeof(TransferettoSftpSession).GetProperty(nameof(TransferettoSftpSession.HostKeyInfo))!;
        hostKeyInfoProperty.SetValue(session, hostKeyInfo);

        Assert.Equal("sftp.example.com", session.Host);
        Assert.Equal(22, session.Port);
        Assert.False(session.IsConnected);
        Assert.Equal("none", session.Error);
        Assert.NotNull(session.HostKeyInfo);
        Assert.Equal("ssh-ed25519", session.HostKeyInfo!.HostKeyName);
        Assert.Equal(TransferettoSshHostKeyTrustSource.KnownHosts, session.HostKeyInfo.TrustSource);
        Assert.Equal("C:\\temp\\ssh-known-hosts.tsv", session.HostKeyInfo.KnownHostsPath);
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
    public void SftpListingPathFallsBackToWorkingDirectoryOrDot() {
        MethodInfo method = typeof(TransferettoClient).GetMethod("ResolveSftpListingPath", BindingFlags.Static | BindingFlags.NonPublic)!;
        TransferettoSftpSession session = CreateSftpSession(new SftpClient("sftp.example.com", "user", "password"), "none");

        string explicitPath = (string) method.Invoke(null, new object?[] { session, "/pub/example" })!;
        string fallbackPath = (string) method.Invoke(null, new object?[] { session, null })!;

        Assert.Equal("/pub/example", explicitPath);
        Assert.Equal(".", fallbackPath);
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
    public void TransferResultExposesSizeAndElapsedMetadata() {
        DateTime startedUtc = new(2026, 4, 17, 10, 0, 0, DateTimeKind.Utc);
        DateTime completedUtc = startedUtc.AddSeconds(2);
        TransferettoTransferResult result = new() {
            Action = "UploadFile",
            Status = true,
            IsSuccess = true,
            LocalPath = "C:\\temp\\artifact.zip",
            RemotePath = "/incoming/artifact.zip",
            BytesTransferred = 2048,
            TotalBytes = 2048,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc
        };

        Assert.Equal(2048, result.BytesTransferred);
        Assert.Equal(2048, result.TotalBytes);
        Assert.Equal(TimeSpan.FromSeconds(2), result.Elapsed);
    }

    [Fact]
    public void TransferProgressCalculatesPercentComplete() {
        TransferettoTransferProgress progress = new() {
            Action = "UploadFile",
            Protocol = "SFTP",
            Direction = TransferettoTransferDirection.Upload,
            BytesTransferred = 512,
            TotalBytes = 1024
        };

        Assert.Equal("SFTP", progress.Protocol);
        Assert.Equal(TransferettoTransferDirection.Upload, progress.Direction);
        Assert.Equal(50, progress.PercentComplete);
    }

    [Fact]
    public void TransferOptionsExposeProgressAndCancellationDefaults() {
        TransferettoTransferOptions options = new();

        Assert.False(options.CancellationToken.IsCancellationRequested);
        Assert.Null(options.Progress);
        Assert.Equal(65536, options.ProgressIntervalBytes);
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
    public void AtomicLocalWriterPreservesExistingFileWhenWriteFails() {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string localPath = Path.Combine(root, "example.txt");
        File.WriteAllText(localPath, "original");
        MethodInfo method = typeof(TransferettoClient).GetMethod("WriteLocalFileAtomically", BindingFlags.Static | BindingFlags.NonPublic)!;
        Action<FileStream> writer = stream => {
            byte[] partialContent = new byte[] { 1, 2, 3, 4 };
            stream.Write(partialContent, 0, partialContent.Length);
            throw new InvalidOperationException("boom");
        };

        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object?[] { localPath, writer }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("original", File.ReadAllText(localPath));
        Assert.Empty(Directory.GetFiles(root, "*.part"));
    }

    [Fact]
    public void AtomicLocalWriterCommitsFileAndReleasesHandleOnSuccess() {
        string root = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string localPath = Path.Combine(root, "example.txt");
        MethodInfo method = typeof(TransferettoClient).GetMethod("WriteLocalFileAtomically", BindingFlags.Static | BindingFlags.NonPublic)!;
        byte[] content = System.Text.Encoding.UTF8.GetBytes("updated");
        Action<FileStream> writer = stream => {
            stream.Write(content, 0, content.Length);
        };

        method.Invoke(null, new object?[] { localPath, writer });

        Assert.Equal("updated", File.ReadAllText(localPath));
        using (FileStream verificationStream = new(localPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
            Assert.True(verificationStream.CanRead);
            Assert.True(verificationStream.CanWrite);
        }

        Assert.Empty(Directory.GetFiles(root, "*.part"));
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

    private static TransferettoSftpSession CreateSftpSession(SftpClient client, string? error) {
        ConstructorInfo constructor = typeof(TransferettoSftpSession).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoSftpSession) constructor.Invoke(new object?[] { client, error });
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
