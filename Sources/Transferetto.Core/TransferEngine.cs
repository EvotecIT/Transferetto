using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Streams content between any two compatible transfer endpoints.
/// </summary>
public static class TransferEngine {
    /// <summary>
    /// Copies one item between endpoints while calculating a provider-independent SHA-256 receipt.
    /// </summary>
    public static async Task<TransferReceipt> CopyAsync(
        ITransferEndpoint source,
        string sourcePath,
        ITransferEndpoint destination,
        string destinationPath,
        TransferCopyOptions? options = null,
        CancellationToken cancellationToken = default) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination));
        }
        if (string.IsNullOrWhiteSpace(sourcePath)) {
            throw new ArgumentException("A source path is required.", nameof(sourcePath));
        }
        if (string.IsNullOrWhiteSpace(destinationPath)) {
            throw new ArgumentException("A destination path is required.", nameof(destinationPath));
        }

        TransferCopyOptions resolvedOptions = options ?? new TransferCopyOptions();
        DateTimeOffset startedAtUtc = DateTimeOffset.UtcNow;
        Guid correlationId = Guid.NewGuid();

        using TransferReadHandle readHandle = await source.OpenReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        using ProgressHashingReadStream trackedStream = new(
            readHandle.Stream,
            sourcePath,
            destinationPath,
            readHandle.Item.Length,
            resolvedOptions.Progress,
            resolvedOptions.ProgressIntervalBytes);

        TransferWriteOptions writeOptions = CloneWriteOptions(
            resolvedOptions.WriteOptions,
            readHandle.Item);
        TransferWriteResult writeResult = await destination.WriteAsync(
            destinationPath,
            trackedStream,
            readHandle.Item.Length,
            writeOptions,
            cancellationToken).ConfigureAwait(false);

        if (writeResult.WasWritten) {
            trackedStream.Complete();
            if (readHandle.Item.Length.HasValue && trackedStream.BytesRead != readHandle.Item.Length.Value) {
                throw new EndOfStreamException(
                    $"The destination consumed {trackedStream.BytesRead} bytes but the source length is {readHandle.Item.Length.Value}.");
            }
        }
        return new TransferReceipt {
            CorrelationId = correlationId,
            SourceEndpoint = source.DisplayName,
            SourcePath = sourcePath,
            DestinationEndpoint = destination.DisplayName,
            DestinationPath = destinationPath,
            Outcome = writeResult.WasWritten ? TransferReceiptOutcome.Copied : TransferReceiptOutcome.Skipped,
            BytesTransferred = trackedStream.BytesRead,
            Sha256 = writeResult.WasWritten ? trackedStream.Sha256 : null,
            SourceETag = readHandle.Item.ETag,
            DestinationETag = writeResult.Item.ETag,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static TransferWriteOptions CloneWriteOptions(
        TransferWriteOptions options,
        TransferItem sourceItem) {
        TransferWriteOptions clone = new() {
            Mode = options.Mode,
            ContentType = options.ContentType ?? sourceItem.ContentType
        };
        foreach (var pair in sourceItem.Metadata) {
            clone.Metadata[pair.Key] = pair.Value;
        }
        foreach (var pair in options.Metadata) {
            clone.Metadata[pair.Key] = pair.Value;
        }
        return clone;
    }

    private sealed class ProgressHashingReadStream : Stream {
        private readonly Stream _inner;
        private readonly string _sourcePath;
        private readonly string _destinationPath;
        private readonly long? _length;
        private readonly IProgress<TransferProgress>? _progress;
        private readonly long _progressInterval;
        private readonly SHA256 _sha256 = SHA256.Create();
        private long _lastProgress;
        private bool _completed;

        internal ProgressHashingReadStream(
            Stream inner,
            string sourcePath,
            string destinationPath,
            long? length,
            IProgress<TransferProgress>? progress,
            long progressInterval) {
            _inner = inner;
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
            _length = length;
            _progress = progress;
            _progressInterval = Math.Max(1, progressInterval);
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
            int read = _inner.Read(buffer, offset, count);
            Track(buffer, offset, read);
            return read;
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) {
            int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            Track(buffer, offset, read);
            return read;
        }

        internal void Complete() {
            if (_completed) {
                return;
            }
            _sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            Sha256 = BitConverter.ToString(_sha256.Hash!).Replace("-", string.Empty).ToLowerInvariant();
            _completed = true;
            ReportProgress(force: true);
        }

        private void Track(byte[] buffer, int offset, int read) {
            if (read <= 0) {
                Complete();
                return;
            }
            _sha256.TransformBlock(buffer, offset, read, null, 0);
            BytesRead += read;
            ReportProgress(force: false);
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
                Complete();
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
