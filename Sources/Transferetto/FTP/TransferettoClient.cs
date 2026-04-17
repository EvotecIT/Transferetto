using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentFTP;
using FluentFTP.Proxy.SyncProxy;

namespace Transferetto;
/// <summary>
/// Provides reusable FTP, FTPS, SFTP, SCP, and SSH operations for Transferetto consumers.
/// </summary>

public static partial class TransferettoClient {
    /// <summary>
    /// Creates and connects an FTP or FTPS session.
    /// </summary>
    public static TransferettoFtpSession ConnectFtp(TransferettoFtpConnectionOptions options) {
        EnsureNotNull(options, nameof(options));

        FtpClient client = CreateFtpClient(options);
        TransferettoFtpCertificateInfo? certificateInfo = null;

        try {
            ConfigureFtpClient(client, options);
            if (!options.ValidateAnyCertificate) {
                client.ValidateCertificate += (_, args) => {
                    certificateInfo = EvaluateFtpCertificateTrust(options, args);
                    args.Accept = certificateInfo.CanTrust;
                };
            }

            FtpProfile? autoDetectedProfile = null;

            if (options.AutoConnect) {
                autoDetectedProfile = client.AutoConnect();
            } else {
                client.Connect();
            }

            return new TransferettoFtpSession(client, autoDetectedProfile) {
                CertificateInfo = certificateInfo
            };
        } catch {
            client.Dispose();
            throw;
        }
    }
    /// <summary>
    /// Closes an FTP or FTPS session.
    /// </summary>

    public static void DisconnectFtp(TransferettoFtpSession session) {
        EnsureNotNull(session, nameof(session));

        if (session.Client.IsConnected) {
            session.Client.Disconnect();
        }
    }
    /// <summary>
    /// Gets a remote FTP or FTPS directory listing.
    /// </summary>

    public static IReadOnlyList<TransferettoRemoteItem> GetFtpListing(
        TransferettoFtpSession session,
        string? path = null,
        FtpListOption? options = null) {
        EnsureNotNull(session, nameof(session));

        FtpListItem[] items = path switch {
            not null when options.HasValue => session.Client.GetListing(path, options.Value),
            not null => session.Client.GetListing(path),
            _ => session.Client.GetListing()
        };

        return items.Select(TransferettoRemoteItem.FromFtpListItem).ToArray();
    }
    /// <summary>
    /// Uploads one or more files to an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadFtpFiles(
        TransferettoFtpSession session,
        string? remotePath,
        IEnumerable<string>? localPaths,
        IEnumerable<FileInfo>? localFiles,
        FtpRemoteExists remoteExists = FtpRemoteExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        FtpError errorHandling = FtpError.None,
        bool createRemoteDirectory = false) {
        return UploadFtpFiles(session, remotePath, localPaths, localFiles, remoteExists, verifyOptions, errorHandling, createRemoteDirectory, null);
    }
    /// <summary>
    /// Uploads one or more files to an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> UploadFtpFiles(
        TransferettoFtpSession session,
        string? remotePath,
        IEnumerable<string>? localPaths,
        IEnumerable<FileInfo>? localFiles,
        FtpRemoteExists remoteExists,
        FtpVerify verifyOptions,
        FtpError errorHandling,
        bool createRemoteDirectory,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        options?.CancellationToken.ThrowIfCancellationRequested();

        string[] resolvedPaths = localPaths?.Where(static p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? Array.Empty<string>();
        FileInfo[] resolvedFiles = localFiles?.Where(static file => file is not null).ToArray() ?? Array.Empty<FileInfo>();

        if (resolvedPaths.Length == 0 && resolvedFiles.Length == 0) {
            return Array.Empty<TransferettoTransferResult>();
        }

        if (resolvedPaths.Length + resolvedFiles.Length == 1) {
            string localPath = resolvedPaths.FirstOrDefault()
                ?? resolvedFiles[0].FullName;

            long? totalBytes = File.Exists(localPath) ? new FileInfo(localPath).Length : null;
            long bytesTransferred = 0;
            DateTime startedUtc = DateTime.UtcNow;
            long lastReportedBytes = 0;
            Action<FtpProgress>? progress = options is null
                ? null
                : ftpProgress => {
                    bytesTransferred = Math.Max(bytesTransferred, NormalizeTransferredBytes(ftpProgress.TransferredBytes));
                    lastReportedBytes = ReportFtpTransferProgress(
                        options,
                        "UploadFile",
                        TransferettoTransferDirection.Upload,
                        localPath,
                        remotePath,
                        ftpProgress,
                        totalBytes,
                        lastReportedBytes);
                };

            FtpStatus status = session.Client.UploadFile(localPath, remotePath, remoteExists, createRemoteDirectory, verifyOptions, progress);
            if (status == FtpStatus.Success && totalBytes.HasValue) {
                bytesTransferred = Math.Max(bytesTransferred, totalBytes.Value);
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "UploadFile",
                    TransferettoTransferDirection.Upload,
                    localPath,
                    remotePath,
                    bytesTransferred,
                    totalBytes,
                    lastReportedBytes,
                    force: true);
            }

            DateTime completedUtc = DateTime.UtcNow;
            return new[] { FromUploadStatus(localPath, remotePath, status, bytesTransferred, totalBytes, startedUtc, completedUtc) };
        }

        long lastMultiReportedBytes = 0;
        Action<FtpProgress>? multiProgress = options is null
            ? null
            : ftpProgress => {
                lastMultiReportedBytes = ReportFtpTransferProgress(
                    options,
                    "UploadFile",
                    TransferettoTransferDirection.Upload,
                    null,
                    remotePath,
                    ftpProgress,
                    null,
                    lastMultiReportedBytes);
            };

        List<FtpResult> results = resolvedFiles.Length > 0
            ? session.Client.UploadFiles(resolvedFiles, remotePath, remoteExists, createRemoteDirectory, verifyOptions, errorHandling, multiProgress, null)
            : session.Client.UploadFiles(resolvedPaths, remotePath, remoteExists, createRemoteDirectory, verifyOptions, errorHandling, multiProgress, null);

        return results.Select(FromFtpResultUpload).ToArray();
    }
    /// <summary>
    /// Downloads a file from an FTP or FTPS server.
    /// </summary>

    public static TransferettoTransferResult DownloadFtpFile(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None) {
        return DownloadFtpFile(session, localPath, remotePath, localExists, verifyOptions, null);
    }
    /// <summary>
    /// Downloads a file from an FTP or FTPS server.
    /// </summary>

    public static TransferettoTransferResult DownloadFtpFile(
        TransferettoFtpSession session,
        string localPath,
        string remotePath,
        FtpLocalExists localExists,
        FtpVerify verifyOptions,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));
        options?.CancellationToken.ThrowIfCancellationRequested();

        long remoteSize = GetFtpFileSize(session, remotePath, -1);
        long? totalBytes = remoteSize >= 0 ? remoteSize : null;
        long bytesTransferred = 0;
        DateTime startedUtc = DateTime.UtcNow;
        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                bytesTransferred = Math.Max(bytesTransferred, NormalizeTransferredBytes(ftpProgress.TransferredBytes));
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "DownloadFile",
                    TransferettoTransferDirection.Download,
                    localPath,
                    remotePath,
                    ftpProgress,
                    totalBytes,
                    lastReportedBytes);
            };

        FtpStatus status = session.Client.DownloadFile(localPath, remotePath, localExists, verifyOptions, progress);
        if (status == FtpStatus.Success) {
            if (File.Exists(localPath)) {
                bytesTransferred = Math.Max(bytesTransferred, new FileInfo(localPath).Length);
            }

            if (totalBytes.HasValue) {
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "DownloadFile",
                    TransferettoTransferDirection.Download,
                    localPath,
                    remotePath,
                    bytesTransferred,
                    totalBytes,
                    lastReportedBytes,
                    force: true);
            }
        }

        DateTime completedUtc = DateTime.UtcNow;
        return FromDownloadStatus(localPath, remotePath, status, bytesTransferred, totalBytes, startedUtc, completedUtc);
    }
    /// <summary>
    /// Downloads multiple files from an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpFiles(
        TransferettoFtpSession session,
        string localPath,
        IEnumerable<string> remotePaths,
        FtpLocalExists localExists = FtpLocalExists.Skip,
        FtpVerify verifyOptions = FtpVerify.None,
        FtpError errorHandling = FtpError.Stop) {
        return DownloadFtpFiles(session, localPath, remotePaths, localExists, verifyOptions, errorHandling, null);
    }
    /// <summary>
    /// Downloads multiple files from an FTP or FTPS server.
    /// </summary>

    public static IReadOnlyList<TransferettoTransferResult> DownloadFtpFiles(
        TransferettoFtpSession session,
        string localPath,
        IEnumerable<string> remotePaths,
        FtpLocalExists localExists,
        FtpVerify verifyOptions,
        FtpError errorHandling,
        TransferettoTransferOptions? options) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNull(remotePaths, nameof(remotePaths));
        options?.CancellationToken.ThrowIfCancellationRequested();

        string[] resolvedPaths = remotePaths.Where(static path => !string.IsNullOrWhiteSpace(path)).ToArray();
        if (resolvedPaths.Length == 0) {
            return Array.Empty<TransferettoTransferResult>();
        }

        long lastReportedBytes = 0;
        Action<FtpProgress>? progress = options is null
            ? null
            : ftpProgress => {
                lastReportedBytes = ReportFtpTransferProgress(
                    options,
                    "DownloadFile",
                    TransferettoTransferDirection.Download,
                    localPath,
                    null,
                    ftpProgress,
                    null,
                    lastReportedBytes);
            };

        List<FtpResult> results = session.Client.DownloadFiles(localPath, resolvedPaths, localExists, verifyOptions, errorHandling, progress, null);
        return results.Select(result => new TransferettoTransferResult {
            Action = "DownloadFile",
            Status = result.IsSuccess,
            IsSuccess = result.IsSuccess,
            IsSkipped = result.IsSkipped,
            IsSkippedByRule = result.IsSkippedByRule,
            IsFailed = result.IsFailed,
            LocalPath = localPath,
            RemotePath = result.RemotePath,
            Message = result.Exception?.Message ?? (result.IsSkipped ? "Skipped" : "Success")
        }).ToArray();
    }
    /// <summary>
    /// Opens a streamed FTP or FTPS file session.
    /// </summary>

    public static TransferettoFtpStreamSession OpenFtpStream(
        TransferettoFtpSession session,
        string path,
        TransferettoFtpStreamMode mode = TransferettoFtpStreamMode.Read) {
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(path, nameof(path));

        Stream stream = mode switch {
            TransferettoFtpStreamMode.Write => session.Client.OpenWrite(path),
            TransferettoFtpStreamMode.Append => session.Client.OpenAppend(path),
            _ => session.Client.OpenRead(path)
        };

        return new TransferettoFtpStreamSession(session, stream, path, mode);
    }
    /// <summary>
    /// Reads a chunk from an FTP or FTPS stream.
    /// </summary>

    public static TransferettoFtpStreamReadResult ReadFtpStream(TransferettoFtpStreamSession session, int count = 4096) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);

        if (!session.CanRead) {
            throw new InvalidOperationException("The FTP stream is not readable.");
        }

        int resolvedCount = count > 0 ? count : 4096;
        byte[] buffer = new byte[resolvedCount];
        int bytesRead = session.Stream.Read(buffer, 0, buffer.Length);
        if (bytesRead != buffer.Length) {
            Array.Resize(ref buffer, bytesRead);
        }

        long position = session.CanSeek ? session.Position : bytesRead;
        return new TransferettoFtpStreamReadResult {
            Status = true,
            Path = session.RemotePath,
            Data = buffer,
            BytesRead = bytesRead,
            Position = position,
            EndOfStream = bytesRead == 0 || (session.CanSeek && session.Position >= session.Length)
        };
    }
    /// <summary>
    /// Writes a chunk to an FTP or FTPS stream.
    /// </summary>

    public static TransferettoFtpStreamWriteResult WriteFtpStream(
        TransferettoFtpStreamSession session,
        byte[] content,
        bool flush = false) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);
        EnsureNotNull(content, nameof(content));

        if (!session.CanWrite) {
            throw new InvalidOperationException("The FTP stream is not writable.");
        }

        session.Stream.Write(content, 0, content.Length);
        if (flush) {
            session.Stream.Flush();
        }

        return new TransferettoFtpStreamWriteResult {
            Status = true,
            Path = session.RemotePath,
            BytesWritten = content.Length,
            Position = session.CanSeek ? session.Position : content.Length,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Moves the current position within an FTP or FTPS stream.
    /// </summary>

    public static long SeekFtpStream(TransferettoFtpStreamSession session, long offset, SeekOrigin origin = SeekOrigin.Begin) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);

        if (!session.CanSeek) {
            throw new InvalidOperationException("The FTP stream does not support seeking.");
        }

        return session.Stream.Seek(offset, origin);
    }
    /// <summary>
    /// Flushes buffered data for an FTP or FTPS stream.
    /// </summary>

    public static TransferettoOperationResult FlushFtpStream(TransferettoFtpStreamSession session) {
        EnsureNotNull(session, nameof(session));
        EnsureOpenFtpStream(session);

        session.Stream.Flush();
        return new TransferettoOperationResult {
            Action = "FlushStream",
            Status = true,
            Path = session.RemotePath,
            Message = string.Empty
        };
    }
    /// <summary>
    /// Closes an FTP or FTPS stream session.
    /// </summary>

    public static void CloseFtpStream(TransferettoFtpStreamSession session) {
        EnsureNotNull(session, nameof(session));
        session.Dispose();
    }

    private static FtpClient CreateFtpClient(TransferettoFtpConnectionOptions options) {
        if (options.ProxyOptions is null) {
            return options.FtpProfile is not null
                ? new FtpClient()
                : new FtpClient(options.Server);
        }

        if (string.IsNullOrWhiteSpace(options.ProxyOptions.ProxyHost)) {
            throw new InvalidOperationException("ProxyHost must be provided when ProxyOptions are used.");
        }

        FtpProxyProfile profile = new() {
            ProxyHost = options.ProxyOptions.ProxyHost,
            ProxyPort = options.ProxyOptions.ProxyPort,
            ProxyCredentials = options.ProxyOptions.ProxyCredential,
            FtpHost = options.Server ?? string.Empty,
            FtpPort = options.Port ?? 0
        };

        return options.ProxyOptions.ProxyType switch {
            TransferettoFtpProxyType.FtpClientSocks5Proxy => new FtpClientSocks5Proxy(profile),
            TransferettoFtpProxyType.FtpClientHttp11Proxy => new FtpClientHttp11Proxy(profile),
            TransferettoFtpProxyType.FtpClientSocks4aProxy => new FtpClientSocks4aProxy(profile),
            TransferettoFtpProxyType.FtpClientSocks4Proxy => new FtpClientSocks4Proxy(profile),
            TransferettoFtpProxyType.FtpClientUserAtHostProxy => new FtpClientUserAtHostProxy(profile),
            TransferettoFtpProxyType.FtpClientBlueCoatProxy => new FtpClientBlueCoatProxy(profile),
            _ => throw new InvalidOperationException($"Unsupported proxy type {options.ProxyOptions.ProxyType}.")
        };
    }

    private static long ReportFtpTransferProgress(
        TransferettoTransferOptions? options,
        string action,
        TransferettoTransferDirection direction,
        string? localPath,
        string? remotePath,
        FtpProgress progress,
        long? totalBytes,
        long lastReportedBytes,
        string protocol = "FTP",
        bool force = false) {
        return options is null
            ? lastReportedBytes
            : ReportFtpTransferProgress(
                options,
                action,
                direction,
                progress.LocalPath ?? localPath,
                progress.RemotePath ?? remotePath,
                NormalizeTransferredBytes(progress.TransferredBytes),
                ResolveFtpTotalBytes(progress, totalBytes),
                lastReportedBytes,
                protocol,
                force);
    }

    private static long ReportFtpTransferProgress(
        TransferettoTransferOptions? options,
        string action,
        TransferettoTransferDirection direction,
        string? localPath,
        string? remotePath,
        long transferredBytes,
        long? totalBytes,
        long lastReportedBytes,
        string protocol = "FTP",
        bool force = false) {
        if (options is null) {
            return lastReportedBytes;
        }

        long normalizedTransferredBytes = NormalizeTransferredBytes(transferredBytes);
        return ReportTransferProgress(
            options,
            action,
            protocol,
            direction,
            localPath,
            remotePath,
            (ulong) normalizedTransferredBytes,
            totalBytes,
            lastReportedBytes,
            force);
    }

    private static long? ResolveFtpTotalBytes(FtpProgress progress, long? fallbackTotalBytes) {
        if (fallbackTotalBytes is >= 0) {
            return fallbackTotalBytes.Value;
        }

        long transferredBytes = NormalizeTransferredBytes(progress.TransferredBytes);
        if (progress.Progress <= 0 || transferredBytes <= 0) {
            return null;
        }

        double totalBytes = transferredBytes * 100d / progress.Progress;
        if (double.IsNaN(totalBytes) || double.IsInfinity(totalBytes) || totalBytes <= 0 || totalBytes > long.MaxValue) {
            return null;
        }

        return (long) Math.Round(totalBytes, MidpointRounding.AwayFromZero);
    }

    private static long NormalizeTransferredBytes(long transferredBytes) {
        return transferredBytes < 0 ? 0 : transferredBytes;
    }

    private static void ConfigureFtpClient(FtpClient client, TransferettoFtpConnectionOptions options) {
        if (options.ValidateAnyCertificate && HasExpectedCertificateThumbprints(options)) {
            throw new InvalidOperationException("ValidateAnyCertificate cannot be combined with ExpectedCertificateThumbprints because accept-any validation bypasses certificate pinning.");
        }

        if (options.FtpProfile is not null) {
            client.LoadProfile(options.FtpProfile);
        }

        if (options.Credential is not null) {
            client.Credentials = options.Credential;
        }

        if (options.Port.HasValue) {
            client.Port = options.Port.Value;
        }

        if (options.DataConnectionType.HasValue) {
            client.Config.DataConnectionType = options.DataConnectionType.Value;
        }

        if (options.EncryptionMode is { Length: > 0 }) {
            client.Config.EncryptionMode = options.EncryptionMode[0];
        }

        // FluentFTP 54 removed the configurable SSL buffering toggle.
        // We keep the option on our model temporarily for compatibility,
        // but there is no equivalent runtime setting to apply here.

        if (options.DisableDataConnectionEncryption) {
            client.Config.DataConnectionEncryption = false;
        }

        if (options.DisableValidateCertificateRevocation) {
            client.Config.ValidateCertificateRevocation = false;
        }

        if (options.ValidateAnyCertificate) {
            client.Config.ValidateAnyCertificate = true;
        }

        if (options.ConnectTimeout.HasValue) {
            client.Config.ConnectTimeout = options.ConnectTimeout.Value;
        }

        if (options.ReadTimeout.HasValue) {
            client.Config.ReadTimeout = options.ReadTimeout.Value;
        }

        if (options.DataConnectionConnectTimeout.HasValue) {
            client.Config.DataConnectionConnectTimeout = options.DataConnectionConnectTimeout.Value;
        }

        if (options.DataConnectionReadTimeout.HasValue) {
            client.Config.DataConnectionReadTimeout = options.DataConnectionReadTimeout.Value;
        }

        if (options.RetryAttempts.HasValue) {
            client.Config.RetryAttempts = options.RetryAttempts.Value;
        }

        if (options.TransferChunkSize.HasValue) {
            client.Config.TransferChunkSize = options.TransferChunkSize.Value;
        }

        if (options.LocalFileBufferSize.HasValue) {
            client.Config.LocalFileBufferSize = options.LocalFileBufferSize.Value;
        }

        if (options.InternetProtocolVersions.HasValue) {
            client.Config.InternetProtocolVersions = options.InternetProtocolVersions.Value;
        }

        if (options.UploadRateLimit.HasValue) {
            client.Config.UploadRateLimit = options.UploadRateLimit.Value;
        }

        if (options.DownloadRateLimit.HasValue) {
            client.Config.DownloadRateLimit = options.DownloadRateLimit.Value;
        }

        if (options.UploadDataType.HasValue) {
            client.Config.UploadDataType = options.UploadDataType.Value;
        }

        if (options.DownloadDataType.HasValue) {
            client.Config.DownloadDataType = options.DownloadDataType.Value;
        }

        if (options.ListingDataType.HasValue) {
            client.Config.ListingDataType = options.ListingDataType.Value;
        }

        if (options.FXPDataType.HasValue) {
            client.Config.FXPDataType = options.FXPDataType.Value;
        }

        if (options.FXPProgressInterval.HasValue) {
            client.Config.FXPProgressInterval = options.FXPProgressInterval.Value;
        }

        if (options.ActivePorts is { Length: > 0 }) {
            client.Config.ActivePorts = options.ActivePorts;
        }

        if (options.PassiveBlockedPorts is { Length: > 0 }) {
            client.Config.PassiveBlockedPorts = options.PassiveBlockedPorts;
        }

        if (options.PassiveMaxAttempts.HasValue) {
            client.Config.PassiveMaxAttempts = options.PassiveMaxAttempts.Value;
        }

        if (!string.IsNullOrWhiteSpace(options.EncodingName)) {
            client.Encoding = Encoding.GetEncoding(options.EncodingName!);
        }

        if (options.SendHost) {
            client.Config.SendHost = true;
        }

        if (options.SocketKeepAlive) {
            client.Config.SocketKeepAlive = true;
        }

        TransferettoFtpTraceOptions? trace = options.TraceOptions ?? TransferettoRuntimeSettings.FtpTraceOptions;
        if (trace is not null) {
            client.Config.LogHost = trace.LogHost;
            client.Config.LogUserName = trace.LogUserName;
            client.Config.LogPassword = trace.LogPassword;
            client.Config.LogToConsole = trace.LogToConsole;
        }
    }

    private static TransferettoFtpCertificateInfo EvaluateFtpCertificateTrust(TransferettoFtpConnectionOptions options, FtpSslValidationEventArgs args) {
        TransferettoFtpCertificateInfo certificateInfo = CreateFtpCertificateInfo(args);

        if (HasExpectedCertificateThumbprints(options)) {
            certificateInfo.CanTrust = CertificateThumbprintMatches(options.ExpectedCertificateThumbprints!, certificateInfo);
            certificateInfo.TrustSource = certificateInfo.CanTrust
                ? TransferettoFtpCertificateTrustSource.ExpectedThumbprint
                : TransferettoFtpCertificateTrustSource.None;
            return certificateInfo;
        }

        return options.CertificatePolicy switch {
            TransferettoFtpCertificatePolicy.KnownCertificates => EvaluateKnownCertificateTrust(options, certificateInfo, false),
            TransferettoFtpCertificatePolicy.TrustOnFirstUse => EvaluateKnownCertificateTrust(options, certificateInfo, true),
            _ => EvaluatePolicyChainTrust(args, certificateInfo)
        };
    }

    private static TransferettoFtpCertificateInfo EvaluatePolicyChainTrust(FtpSslValidationEventArgs args, TransferettoFtpCertificateInfo certificateInfo) {
        certificateInfo.CanTrust = args.PolicyErrors == SslPolicyErrors.None;
        certificateInfo.TrustSource = certificateInfo.CanTrust
            ? TransferettoFtpCertificateTrustSource.PolicyChain
            : TransferettoFtpCertificateTrustSource.None;
        return certificateInfo;
    }

    private static TransferettoFtpCertificateInfo EvaluateKnownCertificateTrust(
        TransferettoFtpConnectionOptions options,
        TransferettoFtpCertificateInfo certificateInfo,
        bool trustOnFirstUse) {
        string knownCertificatesPath = ResolveKnownCertificatesPath(options);
        certificateInfo.KnownCertificatesPath = knownCertificatesPath;

        List<TransferettoFtpKnownCertificateEntry> entries = LoadKnownCertificates(knownCertificatesPath);
        TransferettoFtpKnownCertificateEntry[] matchingEntries = entries
            .Where(entry => string.Equals(entry.Host, options.Server, StringComparison.OrdinalIgnoreCase) && entry.Port == ResolveFtpPort(options))
            .ToArray();

        if (matchingEntries.Length == 0) {
            if (!trustOnFirstUse) {
                certificateInfo.CanTrust = false;
                certificateInfo.TrustSource = TransferettoFtpCertificateTrustSource.None;
                return certificateInfo;
            }

            entries.Add(CreateKnownCertificateEntry(options, certificateInfo));
            SaveKnownCertificates(knownCertificatesPath, entries);
            certificateInfo.CanTrust = true;
            certificateInfo.TrustSource = TransferettoFtpCertificateTrustSource.TrustOnFirstUse;
            certificateInfo.WasPersisted = true;
            return certificateInfo;
        }

        TransferettoFtpKnownCertificateEntry? trustedEntry = matchingEntries.FirstOrDefault(entry => KnownCertificateMatches(entry, certificateInfo));
        bool isTrusted = trustedEntry is not null;
        if (trustedEntry is not null) {
            trustedEntry.LastSeenUtc = DateTime.UtcNow.ToString("O");
            SaveKnownCertificates(knownCertificatesPath, entries);
        }

        certificateInfo.CanTrust = isTrusted;
        certificateInfo.TrustSource = isTrusted
            ? TransferettoFtpCertificateTrustSource.KnownCertificates
            : TransferettoFtpCertificateTrustSource.None;
        return certificateInfo;
    }

    private static TransferettoFtpCertificateInfo CreateFtpCertificateInfo(FtpSslValidationEventArgs args) {
        if (args.Certificate is null) {
            return new TransferettoFtpCertificateInfo {
                PolicyErrors = args.PolicyErrors.ToString(),
                CanTrust = false,
                TrustSource = TransferettoFtpCertificateTrustSource.None
            };
        }

        X509Certificate2 certificate = args.Certificate as X509Certificate2 ?? new X509Certificate2(args.Certificate);
        try {
            return new TransferettoFtpCertificateInfo {
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                ThumbprintSHA1 = NormalizeCertificateThumbprint(certificate.Thumbprint),
                ThumbprintSHA256 = GetCertificateHash(certificate, SHA256.Create()),
                NotBefore = certificate.NotBefore,
                NotAfter = certificate.NotAfter,
                PolicyErrors = args.PolicyErrors == SslPolicyErrors.None ? null : args.PolicyErrors.ToString()
            };
        } finally {
            if (!ReferenceEquals(certificate, args.Certificate)) {
                certificate.Dispose();
            }
        }
    }

    private static bool CertificateThumbprintMatches(IEnumerable<string> expectedThumbprints, TransferettoFtpCertificateInfo certificateInfo) {
        HashSet<string> actualThumbprints = new(StringComparer.OrdinalIgnoreCase);
        AddCertificateThumbprint(actualThumbprints, "SHA1", certificateInfo.ThumbprintSHA1);
        AddCertificateThumbprint(actualThumbprints, "SHA256", certificateInfo.ThumbprintSHA256);

        foreach (string expectedThumbprint in expectedThumbprints.Where(static value => !string.IsNullOrWhiteSpace(value))) {
            string normalizedExpected = NormalizeCertificateThumbprint(expectedThumbprint);
            if (actualThumbprints.Contains(normalizedExpected)) {
                return true;
            }
        }

        return false;
    }

    private static bool KnownCertificateMatches(TransferettoFtpKnownCertificateEntry entry, TransferettoFtpCertificateInfo certificateInfo) {
        return CertificateThumbprintMatches(
            new[] {
                entry.ThumbprintSHA1,
                entry.ThumbprintSHA256
            }.Where(static value => !string.IsNullOrWhiteSpace(value))!,
            certificateInfo);
    }

    private static bool HasExpectedCertificateThumbprints(TransferettoFtpConnectionOptions options) {
        return options.ExpectedCertificateThumbprints?.Any(static value => !string.IsNullOrWhiteSpace(value)) == true;
    }

    private static string ResolveKnownCertificatesPath(TransferettoFtpConnectionOptions options) {
        if (!string.IsNullOrWhiteSpace(options.KnownCertificatesPath)) {
            return options.KnownCertificatesPath!;
        }

        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root)) {
            root = AppDomain.CurrentDomain.BaseDirectory;
        }

        return Path.Combine(root, "Transferetto", "ftps-known-certificates.tsv");
    }

    private static int ResolveFtpPort(TransferettoFtpConnectionOptions options) {
        if (options.Port.HasValue && options.Port.Value > 0) {
            return options.Port.Value;
        }

        return options.EncryptionMode?.Contains(FtpEncryptionMode.Implicit) == true ? 990 : 21;
    }

    private static List<TransferettoFtpKnownCertificateEntry> LoadKnownCertificates(string path) {
        if (!File.Exists(path)) {
            return new List<TransferettoFtpKnownCertificateEntry>();
        }

        List<TransferettoFtpKnownCertificateEntry> entries = new();
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

            entries.Add(new TransferettoFtpKnownCertificateEntry {
                Host = parts[0],
                Port = port,
                Subject = parts[2],
                Issuer = parts[3],
                ThumbprintSHA1 = parts[4],
                ThumbprintSHA256 = parts[5],
                FirstSeenUtc = parts[6],
                LastSeenUtc = parts[7]
            });
        }

        return entries;
    }

    private static void SaveKnownCertificates(string path, IEnumerable<TransferettoFtpKnownCertificateEntry> entries) {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        string[] lines = entries.Select(SerializeKnownCertificateEntry).ToArray();
        File.WriteAllLines(path, lines);
    }

    private static string SerializeKnownCertificateEntry(TransferettoFtpKnownCertificateEntry entry) {
        return string.Join("\t", new[] {
            SanitizeKnownCertificateValue(entry.Host),
            entry.Port.ToString(),
            SanitizeKnownCertificateValue(entry.Subject),
            SanitizeKnownCertificateValue(entry.Issuer),
            SanitizeKnownCertificateValue(entry.ThumbprintSHA1),
            SanitizeKnownCertificateValue(entry.ThumbprintSHA256),
            SanitizeKnownCertificateValue(entry.FirstSeenUtc),
            SanitizeKnownCertificateValue(entry.LastSeenUtc)
        });
    }

    private static TransferettoFtpKnownCertificateEntry CreateKnownCertificateEntry(
        TransferettoFtpConnectionOptions options,
        TransferettoFtpCertificateInfo certificateInfo) {
        string now = DateTime.UtcNow.ToString("O");
        return new TransferettoFtpKnownCertificateEntry {
            Host = options.Server ?? string.Empty,
            Port = ResolveFtpPort(options),
            Subject = certificateInfo.Subject,
            Issuer = certificateInfo.Issuer,
            ThumbprintSHA1 = certificateInfo.ThumbprintSHA1,
            ThumbprintSHA256 = certificateInfo.ThumbprintSHA256,
            FirstSeenUtc = now,
            LastSeenUtc = now
        };
    }

    private static string SanitizeKnownCertificateValue(string? value) {
        return (value ?? string.Empty).Replace("\t", " ").Trim();
    }

    private static void AddCertificateThumbprint(HashSet<string> thumbprints, string algorithm, string? thumbprint) {
        if (string.IsNullOrWhiteSpace(thumbprint)) {
            return;
        }

        string normalized = NormalizeCertificateThumbprint(thumbprint!);
        thumbprints.Add(normalized);
        if (!normalized.Contains(":", StringComparison.Ordinal)) {
            thumbprints.Add($"{algorithm}:{normalized}");
        }
    }

    private static string NormalizeCertificateThumbprint(string thumbprint) {
        string trimmed = thumbprint.Trim();
        int prefixIndex = trimmed.IndexOf(':');
        string? algorithm = null;
        if (prefixIndex > 0) {
            string prefix = trimmed.Substring(0, prefixIndex);
            if (prefix.Equals("SHA1", StringComparison.OrdinalIgnoreCase) || prefix.Equals("SHA256", StringComparison.OrdinalIgnoreCase)) {
                algorithm = prefix.ToUpperInvariant();
                trimmed = trimmed.Substring(prefixIndex + 1);
            }
        }

        string normalized = new(trimmed.Where(IsHexDigit).Select(static value => char.ToUpperInvariant(value)).ToArray());
        if (algorithm is not null) {
            return $"{algorithm}:{normalized}";
        }

        return normalized.Length switch {
            40 => "SHA1:" + normalized,
            64 => "SHA256:" + normalized,
            _ => normalized
        };
    }

    private static string GetCertificateHash(X509Certificate2 certificate, HashAlgorithm algorithm) {
        using (algorithm) {
            byte[] hash = algorithm.ComputeHash(certificate.RawData);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }

    private static bool IsHexDigit(char value) {
        return (value >= '0' && value <= '9') ||
            (value >= 'a' && value <= 'f') ||
            (value >= 'A' && value <= 'F');
    }

    private static TransferettoTransferResult FromUploadStatus(string? localPath, string? remotePath, FtpStatus status) {
        return FromUploadStatus(localPath, remotePath, status, null, null, null, null);
    }

    private static TransferettoTransferResult FromUploadStatus(
        string? localPath,
        string? remotePath,
        FtpStatus status,
        long? bytesTransferred,
        long? totalBytes,
        DateTime? startedUtc,
        DateTime? completedUtc) {
        bool isSuccess = status is FtpStatus.Success or FtpStatus.Skipped;
        bool isSkipped = status == FtpStatus.Skipped;

        return new TransferettoTransferResult {
            Action = "UploadFile",
            Status = isSuccess,
            IsSuccess = isSuccess,
            IsSkipped = isSkipped,
            IsSkippedByRule = false,
            IsFailed = !isSuccess,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = status.ToString()
        };
    }

    private static TransferettoTransferResult FromDownloadStatus(string? localPath, string? remotePath, FtpStatus status) {
        return FromDownloadStatus(localPath, remotePath, status, null, null, null, null);
    }

    private static TransferettoTransferResult FromDownloadStatus(
        string? localPath,
        string? remotePath,
        FtpStatus status,
        long? bytesTransferred,
        long? totalBytes,
        DateTime? startedUtc,
        DateTime? completedUtc) {
        bool isSuccess = status is FtpStatus.Success or FtpStatus.Skipped;
        bool isSkipped = status == FtpStatus.Skipped;

        return new TransferettoTransferResult {
            Action = "DownloadFile",
            Status = isSuccess,
            IsSuccess = isSuccess,
            IsSkipped = isSkipped,
            IsSkippedByRule = false,
            IsFailed = !isSuccess,
            LocalPath = localPath,
            RemotePath = remotePath,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
            Message = status.ToString()
        };
    }

    private static TransferettoTransferResult FromFtpResultUpload(FtpResult result) {
        return new TransferettoTransferResult {
            Action = "UploadFile",
            Status = result.IsSuccess,
            IsSuccess = result.IsSuccess,
            IsSkipped = result.IsSkipped,
            IsSkippedByRule = result.IsSkippedByRule,
            IsFailed = result.IsFailed,
            LocalPath = result.LocalPath,
            RemotePath = result.RemotePath,
            BytesTransferred = result.Size >= 0 && result.IsSuccess && !result.IsSkipped ? result.Size : null,
            TotalBytes = result.Size >= 0 ? result.Size : null,
            Message = result.Exception?.Message ?? (result.IsSkipped ? "Skipped" : "Success")
        };
    }

    private static void EnsureNotNull(object? value, string paramName) {
        if (value is null) {
            throw new ArgumentNullException(paramName);
        }
    }

    private static void EnsureNotNullOrWhiteSpace(string? value, string paramName) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }
    }

    private static void EnsureOpenFtpStream(TransferettoFtpStreamSession session) {
        if (session.IsDisposed) {
            throw new ObjectDisposedException(nameof(TransferettoFtpStreamSession), "The FTP stream has already been closed.");
        }
    }
}
