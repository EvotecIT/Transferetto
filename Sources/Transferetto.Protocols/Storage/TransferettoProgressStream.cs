using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto;

/// <summary>Reports progress from awaited stream I/O, where cancellation and sink exceptions reach the operation owner.</summary>
internal sealed class TransferettoProgressStream : Stream {
    private readonly Stream _inner;
    private readonly Action<ulong> _report;
    private ulong _bytes;
    internal TransferettoProgressStream(Stream inner, Action<ulong> report) { _inner = inner; _report = report; }
    public override bool CanRead => _inner.CanRead;
    public override bool CanWrite => _inner.CanWrite;
    public override bool CanSeek => _inner.CanSeek;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override int Read(byte[] buffer, int offset, int count) { int read = _inner.Read(buffer, offset, count); Report(read); return read; }
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false); Report(read); return read;
    }
    public override void Write(byte[] buffer, int offset, int count) { _inner.Write(buffer, offset, count); Report(count); }
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false); Report(count);
    }
#if NET8_0_OR_GREATER
    public override int Read(Span<byte> buffer) { int read = _inner.Read(buffer); Report(read); return read; }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        int read = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false); Report(read); return read;
    }
    public override void Write(ReadOnlySpan<byte> buffer) { _inner.Write(buffer); Report(buffer.Length); }
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
        await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false); Report(buffer.Length);
    }
#endif
    private void Report(int count) { if (count > 0) { _bytes = checked(_bytes + (ulong)count); _report(_bytes); } }
    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    // The caller owns the wrapped stream and disposes it after the protocol operation has completed.
}
