using System;
using System.IO;

namespace Transferetto;
/// <summary>
/// Represents an open FTP stream session.
/// </summary>

public sealed class TransferettoFtpStreamSession : IDisposable {
    private bool _disposed;

    internal TransferettoFtpStreamSession(TransferettoFtpSession session, Stream stream, string remotePath, TransferettoFtpStreamMode mode) {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
        Mode = mode;
    }

    internal TransferettoFtpSession Session { get; }

    internal Stream Stream { get; }
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

    public TransferettoFtpStreamMode Mode { get; }
    /// <summary>
    /// Gets the position.
    /// </summary>

    public long Position => Stream.CanSeek ? Stream.Position : 0;
    /// <summary>
    /// Gets the length.
    /// </summary>

    public long Length => Stream.CanSeek ? Stream.Length : 0;
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
    /// Releases resources held by the FTP or FTPS session.
    /// </summary>

    public void Dispose() {
        if (_disposed) {
            return;
        }

        Stream.Dispose();
        _disposed = true;
    }
}
