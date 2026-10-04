using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

internal sealed class BoundedReadStream : Stream {
    private readonly Stream _inner;
    private long _remaining;

    internal BoundedReadStream(Stream inner, long length) {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (length < 0) { throw new ArgumentOutOfRangeException(nameof(length)); }
        _remaining = length;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) {
        if (_remaining == 0) { return 0; }
        int read = _inner.Read(buffer, offset, (int)Math.Min(count, _remaining));
        _remaining -= read;
        return read;
    }
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        if (_remaining == 0) { return 0; }
        int read = await _inner.ReadAsync(buffer, offset, (int)Math.Min(count, _remaining), cancellationToken).ConfigureAwait(false);
        _remaining -= read;
        return read;
    }
#if NET8_0_OR_GREATER
    public override int Read(Span<byte> buffer) {
        if (_remaining == 0) { return 0; }
        int read = _inner.Read(buffer.Slice(0, (int)Math.Min(buffer.Length, _remaining)));
        _remaining -= read;
        return read;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        if (_remaining == 0) { return 0; }
        int read = await _inner.ReadAsync(buffer.Slice(0, (int)Math.Min(buffer.Length, _remaining)), cancellationToken).ConfigureAwait(false);
        _remaining -= read;
        return read;
    }
#endif
    protected override void Dispose(bool disposing) {
        if (disposing) { _inner.Dispose(); }
        base.Dispose(disposing);
    }
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
