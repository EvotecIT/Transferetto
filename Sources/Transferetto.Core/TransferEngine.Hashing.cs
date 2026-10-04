using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

public static partial class TransferEngine {
    private sealed class ProgressHashingReadStream : Stream {
        private readonly Stream _inner;
        private readonly string _sourcePath;
        private readonly string _destinationPath;
        private readonly long? _length;
        private readonly IProgress<TransferProgress>? _progress;
        private readonly long _progressInterval;
        private readonly string? _expectedSha256;
#if NET8_0_OR_GREATER
        private readonly IncrementalHash _sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
#else
        private readonly SHA256 _sha256 = SHA256.Create();
#endif
        private long _lastProgress;
        private bool _completed;

        internal ProgressHashingReadStream(
            Stream inner,
            string sourcePath,
            string destinationPath,
            long? length,
            IProgress<TransferProgress>? progress,
            long progressInterval,
            string? expectedSha256 = null) {
            _inner = inner;
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
            _length = length;
            _progress = progress;
            _progressInterval = Math.Max(1, progressInterval);
            _expectedSha256 = expectedSha256;
        }

        internal long BytesRead { get; private set; }
        internal string Sha256 { get; private set; } = string.Empty;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length ?? throw new NotSupportedException();
        public override long Position {
            get => BytesRead;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (count == 0) { return _inner.Read(buffer, offset, count); }
            int read = _inner.Read(buffer, offset, count);
            Track(buffer, offset, read);
            return read;
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            if (count == 0) { return await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false); }
            int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            Track(buffer, offset, read);
            return read;
        }

#if NET8_0_OR_GREATER
        public override int Read(Span<byte> buffer) {
            if (buffer.IsEmpty) { return _inner.Read(buffer); }
            int read = _inner.Read(buffer);
            Track(buffer.Slice(0, read), read);
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
            if (buffer.IsEmpty) { return await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false); }
            int read = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            Track(buffer.Span.Slice(0, read), read);
            return read;
        }

        private void Track(ReadOnlySpan<byte> content, int read) {
            if (!ValidateRead(read)) { return; }
            _sha256.AppendData(content);
            if (_length.HasValue && BytesRead == _length.Value) { Complete(); }
            ReportProgress(force: false);
        }
#endif

        internal void Complete() {
            if (_completed) {
                return;
            }
#if NET8_0_OR_GREATER
            Sha256 = Convert.ToHexString(_sha256.GetHashAndReset()).ToLowerInvariant();
#else
            _sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            Sha256 = BitConverter.ToString(_sha256.Hash!).Replace("-", string.Empty).ToLowerInvariant();
#endif
            _completed = true;
            if (_expectedSha256 != null && !string.Equals(Sha256, _expectedSha256, StringComparison.Ordinal)) {
                throw new InvalidDataException("The source content does not match the expected SHA-256 digest.");
            }
            ReportProgress(force: true);
        }

        private void Track(byte[] buffer, int offset, int read) {
            if (!ValidateRead(read)) { return; }
#if NET8_0_OR_GREATER
            _sha256.AppendData(buffer, offset, read);
#else
            _sha256.TransformBlock(buffer, offset, read, null, 0);
#endif
            if (_length.HasValue && BytesRead == _length.Value) { Complete(); }
            ReportProgress(force: false);
        }

        private bool ValidateRead(int read) {
            if (read <= 0) {
                if (_length.HasValue && BytesRead != _length.Value) {
                    throw new EndOfStreamException(
                        $"The source produced {BytesRead} bytes but reported a length of {_length.Value}.");
                }
                Complete();
                return false;
            }
            long nextBytesRead = checked(BytesRead + read);
            if (_length.HasValue && nextBytesRead > _length.Value) {
                throw new EndOfStreamException(
                    $"The source produced more than its reported length of {_length.Value} bytes.");
            }
            BytesRead = nextBytesRead;
            return true;
        }

        private void ReportProgress(bool force) {
            if (_progress == null || (!force && BytesRead - _lastProgress < _progressInterval)) {
                return;
            }
            _lastProgress = BytesRead;
            _progress.Report(new TransferProgress {
                SourcePath = _sourcePath,
                DestinationPath = _destinationPath,
                BytesTransferred = BytesRead,
                TotalBytes = _length
            });
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _sha256.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
