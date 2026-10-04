using System;
using System.Net;
using FluentFTP;
using Transferetto.AzureBlob;
using Transferetto.Core;
using Transferetto.S3;

namespace Transferetto.Cli;

internal sealed class CliEndpoint : IDisposable {
    private readonly IDisposable? _owner;

    private CliEndpoint(ITransferEndpoint endpoint) {
        Endpoint = endpoint;
        _owner = endpoint as IDisposable;
    }

    internal ITransferEndpoint Endpoint { get; }

    internal static CliEndpoint Open(string value) {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) {
            throw new ArgumentException("An absolute endpoint URI without credentials or query is required.", nameof(value));
        }
        string prefix = Uri.UnescapeDataString(uri.AbsolutePath).Trim('/');
        ITransferEndpoint endpoint = uri.Scheme switch {
            "file" => new FileSystemTransferEndpoint(uri.LocalPath),
            "s3" => CreateS3(uri.Host, prefix),
            "azureblob" => CreateAzure(uri.Host, prefix),
            "sftp" => CreateSftp(uri, prefix),
            "ftp" or "ftps" => CreateFtp(uri, prefix),
            _ => throw new ArgumentException("The endpoint scheme is unsupported.", nameof(value))
        };
        return new CliEndpoint(endpoint);
    }

    private static S3TransferEndpoint CreateS3(string bucket, string prefix) {
        if (string.IsNullOrWhiteSpace(bucket)) { throw new ArgumentException("An S3 bucket is required."); }
        return new S3TransferEndpoint(new S3EndpointOptions {
            BucketName = bucket,
            Prefix = prefix,
            Region = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_REGION"),
            ServiceUrl = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SERVICE_URL"),
            ForcePathStyle = string.Equals(Environment.GetEnvironmentVariable("TRANSFERETTO_S3_PATH_STYLE"), "true",
                StringComparison.OrdinalIgnoreCase),
            AccessKeyId = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ACCESS_KEY"),
            SecretAccessKey = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SECRET_KEY"),
            SessionToken = Environment.GetEnvironmentVariable("TRANSFERETTO_S3_SESSION_TOKEN")
        });
    }

    private static AzureBlobTransferEndpoint CreateAzure(string container, string prefix) {
        string connection = Environment.GetEnvironmentVariable("TRANSFERETTO_AZURE_CONNECTION_STRING")
            ?? throw new ArgumentException("TRANSFERETTO_AZURE_CONNECTION_STRING is required.");
        if (string.IsNullOrWhiteSpace(container)) { throw new ArgumentException("An Azure container is required."); }
        return new AzureBlobTransferEndpoint(connection, container, prefix);
    }

    private static SftpTransferEndpoint CreateSftp(Uri uri, string prefix) {
        string? user = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_USER");
        string? knownHosts = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_KNOWN_HOSTS");
        string? fingerprint = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_HOST_KEY");
        if (string.IsNullOrWhiteSpace(user) || (string.IsNullOrWhiteSpace(knownHosts) && string.IsNullOrWhiteSpace(fingerprint))) {
            throw new ArgumentException("SFTP requires a user and an explicit host key or known-hosts file.");
        }
        return new SftpTransferEndpoint(new TransferettoSftpConnectionOptions {
            Server = uri.Host,
            Port = uri.IsDefaultPort ? null : uri.Port,
            UserName = user,
            Password = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_PASSWORD"),
            PrivateKeyPath = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_PRIVATE_KEY"),
            PrivateKeyPassphrase = Environment.GetEnvironmentVariable("TRANSFERETTO_SFTP_KEY_PASSPHRASE"),
            KnownHostsPath = knownHosts,
            ExpectedHostKeyFingerprints = string.IsNullOrWhiteSpace(fingerprint) ? null : new[] { fingerprint },
            HostKeyPolicy = TransferettoSshHostKeyPolicy.KnownHosts
        }, prefix);
    }

    private static FtpTransferEndpoint CreateFtp(Uri uri, string prefix) {
        string? user = Environment.GetEnvironmentVariable("TRANSFERETTO_FTP_USER");
        string? password = Environment.GetEnvironmentVariable("TRANSFERETTO_FTP_PASSWORD");
        if (string.IsNullOrWhiteSpace(user) || password == null) {
            throw new ArgumentException("FTP and FTPS require TRANSFERETTO_FTP_USER and TRANSFERETTO_FTP_PASSWORD.");
        }
        TransferettoFtpConnectionOptions options = new() {
            Server = uri.Host,
            Port = uri.IsDefaultPort ? null : uri.Port,
            Credential = new NetworkCredential(user, password),
            EncryptionMode = uri.Scheme == "ftps" ? new[] { FtpEncryptionMode.Explicit }
                : new[] { FtpEncryptionMode.None }
        };
        return new FtpTransferEndpoint(options, prefix);
    }

    public void Dispose() => _owner?.Dispose();
}
