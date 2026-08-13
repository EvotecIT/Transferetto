using System;
using Renci.SshNet.Sftp;

namespace Transferetto;
/// <summary>
/// Represents an open SFTP stream session.
/// </summary>

public sealed class TransferettoSftpStreamSession : IDisposable {
    private bool _disposed;

    internal TransferettoSftpStreamSession(TransferettoSftpSession session, SftpFileStream stream, string remotePath, TransferettoSftpStreamMode mode) {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
        Mode = mode;
        OpenedPosition = stream.CanSeek ? stream.Position : 0;
    }

    internal TransferettoSftpSession Session { get; }

    internal SftpFileStream Stream { get; }

    internal long OpenedPosition { get; }

    internal long ProcessedBytes { get; set; }

    internal long LastReportedBytes { get; set; }
    /// <summary>
    /// Gets the remote host name.
    /// </summary>

    public string Host => Session.Host;
    /// <summary>
    /// Gets the remote port number.
    /// </summary>

    public int Port => Session.Port;
    /// <summary>
    /// Gets the remote path.
    /// </summary>

    public string RemotePath { get; }
    /// <summary>
    /// Gets the mode.
    /// </summary>

    public TransferettoSftpStreamMode Mode { get; }
    /// <summary>
    /// Gets the position.
    /// </summary>

    public long Position => Stream.Position;
    /// <summary>
    /// Gets the length.
    /// </summary>

    public long Length => Stream.Length;
    /// <summary>
    /// Gets a value indicating whether read.
    /// </summary>

    public bool CanRead => !_disposed && Stream.CanRead;
    /// <summary>
    /// Gets a value indicating whether write.
    /// </summary>

    public bool CanWrite => !_disposed && Stream.CanWrite;
    /// <summary>
    /// Gets a value indicating whether seek.
    /// </summary>

    public bool CanSeek => !_disposed && Stream.CanSeek;
    /// <summary>
    /// Gets a value indicating whether disposed.
    /// </summary>

    public bool IsDisposed => _disposed;
    /// <summary>
    /// Releases resources held by the SFTP session.
    /// </summary>

    public void Dispose() {
        if (_disposed) {
            return;
        }

        Stream.Dispose();
        _disposed = true;
    }
}
