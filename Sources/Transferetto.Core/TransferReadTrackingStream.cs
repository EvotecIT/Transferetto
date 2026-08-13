using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Wraps a readable stream and records how many bytes its consumer actually reads.
/// </summary>
/// <remarks>
/// Seekable streams remain seekable so upload providers can retry by rewinding them. Re-reading an already consumed
/// range does not increase <see cref="BytesRead"/>; non-seekable streams are counted cumulatively.
/// </remarks>
public sealed class TransferReadTrackingStream : Stream {
    private readonly Stream _inner;
    private readonly bool _leaveOpen;
    private readonly long _initialPosition;

    /// <summary>Initializes a tracking stream.</summary>
    /// <param name="inner">The readable stream to wrap.</param>
    /// <param name="leaveOpen">Whether disposing this wrapper leaves <paramref name="inner"/> open.</param>
    public TransferReadTrackingStream(Stream inner, bool leaveOpen = false) {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (!inner.CanRead) {
            throw new ArgumentException("The wrapped stream must be readable.", nameof(inner));
        }
        _leaveOpen = leaveOpen;
        _initialPosition = inner.CanSeek ? inner.Position : 0;
    }

    /// <summary>Gets the number of bytes consumed through this wrapper.</summary>
    public long BytesRead { get; private set; }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => _inner.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => _inner.Length;

    /// <inheritdoc />
    public override long Position {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) {
        int read = _inner.Read(buffer, offset, count);
        Track(read);
        return read;
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken) {
        int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        Track(read);
        return read;
    }

#if NET8_0_OR_GREATER
    /// <inheritdoc />
    public override int Read(Span<byte> buffer) {
        int read = _inner.Read(buffer);
        Track(read);
        return read;
    }

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default) {
        int read = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        Track(read);
        return read;
    }
#endif

    /// <inheritdoc />
    public override int ReadByte() {
        int value = _inner.ReadByte();
        if (value >= 0) {
            Track(1);
        }
        return value;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing) {
        if (disposing && !_leaveOpen) {
            _inner.Dispose();
        }
        base.Dispose(disposing);
    }

    private void Track(int read) {
        if (read <= 0) {
            return;
        }
        if (!_inner.CanSeek) {
            BytesRead = checked(BytesRead + read);
            return;
        }

        long logicalBytesRead = checked(_inner.Position - _initialPosition);
        if (logicalBytesRead > BytesRead) {
            BytesRead = logicalBytesRead;
        }
    }

    /// <inheritdoc />
    public override void Flush() => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
