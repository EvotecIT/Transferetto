using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        try {
            ConfigureFtpClient(client, options);
            FtpProfile? autoDetectedProfile = null;

            if (options.AutoConnect) {
                autoDetectedProfile = client.AutoConnect();
            } else {
                client.Connect();
            }

            return new TransferettoFtpSession(client, autoDetectedProfile);
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
        EnsureNotNull(session, nameof(session));

        string[] resolvedPaths = localPaths?.Where(static p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? Array.Empty<string>();
        FileInfo[] resolvedFiles = localFiles?.Where(static file => file is not null).ToArray() ?? Array.Empty<FileInfo>();

        if (resolvedPaths.Length == 0 && resolvedFiles.Length == 0) {
            return Array.Empty<TransferettoTransferResult>();
        }

        if (resolvedPaths.Length + resolvedFiles.Length == 1) {
            string localPath = resolvedPaths.FirstOrDefault()
                ?? resolvedFiles[0].FullName;

            FtpStatus status = session.Client.UploadFile(localPath, remotePath, remoteExists, createRemoteDirectory, verifyOptions);
            return new[] { FromUploadStatus(localPath, remotePath, status) };
        }

        List<FtpResult> results = resolvedFiles.Length > 0
            ? session.Client.UploadFiles(resolvedFiles, remotePath, remoteExists, createRemoteDirectory, verifyOptions, errorHandling)
            : session.Client.UploadFiles(resolvedPaths, remotePath, remoteExists, createRemoteDirectory, verifyOptions, errorHandling);

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
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNullOrWhiteSpace(remotePath, nameof(remotePath));

        FtpStatus status = session.Client.DownloadFile(localPath, remotePath, localExists, verifyOptions);
        return FromDownloadStatus(localPath, remotePath, status);
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
        EnsureNotNull(session, nameof(session));
        EnsureNotNullOrWhiteSpace(localPath, nameof(localPath));
        EnsureNotNull(remotePaths, nameof(remotePaths));

        string[] resolvedPaths = remotePaths.Where(static path => !string.IsNullOrWhiteSpace(path)).ToArray();
        if (resolvedPaths.Length == 0) {
            return Array.Empty<TransferettoTransferResult>();
        }

        List<FtpResult> results = session.Client.DownloadFiles(localPath, resolvedPaths, localExists, verifyOptions, errorHandling);
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

    private static void ConfigureFtpClient(FtpClient client, TransferettoFtpConnectionOptions options) {
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

    private static TransferettoTransferResult FromUploadStatus(string? localPath, string? remotePath, FtpStatus status) {
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
            Message = status.ToString()
        };
    }

    private static TransferettoTransferResult FromDownloadStatus(string? localPath, string? remotePath, FtpStatus status) {
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
