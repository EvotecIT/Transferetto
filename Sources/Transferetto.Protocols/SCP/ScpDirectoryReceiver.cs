using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto;

/// <summary>Receives SCP directory records with explicit root and file-content boundaries.</summary>
internal sealed class ScpDirectoryReceiver {
    private readonly Stream _records;
    private readonly Stream _acknowledgements;
    private readonly string _root;
    private readonly Action<string, long, long>? _progress;
    private readonly byte[] _singleByte = new byte[1];
    private static readonly Encoding HeaderEncoding = new UTF8Encoding(false, true);

    internal ScpDirectoryReceiver(Stream records, Stream acknowledgements, string root, Action<string, long, long>? progress = null) {
        _records = records;
        _acknowledgements = acknowledgements;
        _root = Path.GetFullPath(root);
        _progress = progress;
    }

    internal async Task ReceiveAsync(CancellationToken cancellationToken) {
        Stack<(string RelativePath, DateTime? Modified, DateTime? Accessed)> directories = new();
        DateTime? modified = null;
        DateTime? accessed = null;
        FileSystemTransferEndpoint destination = new(_root);
        await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
        while (true) {
            cancellationToken.ThrowIfCancellationRequested();
            string record = await ReadHeaderAsync(cancellationToken).ConfigureAwait(false);
            if (record == "E") {
                if (directories.Count == 0) { throw new InvalidDataException("SCP ended a directory before opening the selected root."); }
                var closed = directories.Pop();
                ApplyTimes(TransferFileSystem.ResolveRelativePath(_root, closed.RelativePath, allowEmpty: true), closed.Modified, closed.Accessed);
                await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
                if (directories.Count == 0) { return; }
                continue;
            }
            if (record.StartsWith("T", StringComparison.Ordinal)) {
                string[] values = record.Substring(1).Split(' ');
                if (values.Length != 4) { throw new InvalidDataException("Invalid SCP timestamp record."); }
                modified = ParseTime(values[0], values[1]);
                accessed = ParseTime(values[2], values[3]);
                await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
                continue;
            }
            if (record.Length < 8 || (record[0] != 'D' && record[0] != 'C')) { throw new InvalidDataException("Invalid SCP directory record."); }
            int modeEnd = record.IndexOf(' ');
            int lengthEnd = modeEnd < 0 ? -1 : record.IndexOf(' ', modeEnd + 1);
            if (modeEnd != 5 || lengthEnd < 0 || !ValidMode(record.Substring(1, 4))
                || !long.TryParse(record.Substring(modeEnd + 1, lengthEnd - modeEnd - 1), NumberStyles.None, CultureInfo.InvariantCulture, out long length)) {
                throw new InvalidDataException("Invalid SCP file mode or length.");
            }
            string name = record.Substring(lengthEnd + 1);
            if (name.Length == 0) { throw new InvalidDataException("An SCP item name is required."); }
            if (record[0] == 'D' && directories.Count == 0) {
                if (length != 0) { throw new InvalidDataException("An SCP directory cannot have file content."); }
                // The first remote directory denotes the caller-selected root; its name is not a local path.
                TransferFileSystem.EnsureNoLinkTraversal(_root, _root);
                Directory.CreateDirectory(_root);
                directories.Push((string.Empty, modified, accessed));
            } else {
                if (directories.Count == 0) { throw new InvalidDataException("SCP sent a file outside the selected directory record."); }
                // A protocol name is one component, even when a Unix filename contains a backslash.
                if (name == "." || name == ".." || name.IndexOf('/') >= 0) { throw new InvalidDataException("Invalid SCP item name."); }
                string parent = directories.Peek().RelativePath;
                string relative = parent.Length == 0 ? name : parent + "/" + name;
                string localPath = TransferFileSystem.ResolveRelativePath(_root, relative);
                if (record[0] == 'D') {
                    if (length != 0) { throw new InvalidDataException("An SCP directory cannot have file content."); }
                    Directory.CreateDirectory(localPath);
                    directories.Push((relative, modified, accessed));
                } else {
                    await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
                    using RecordContentStream content = new(this, relative, length);
                    await destination.WriteAsync(relative, content, length,
                        new TransferWriteOptions { Mode = TransferWriteMode.Overwrite }, cancellationToken).ConfigureAwait(false);
                    ApplyTimes(localPath, modified, accessed);
                    await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
                    modified = accessed = null;
                    continue;
                }
            }
            modified = accessed = null;
            await AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void ApplyTimes(string path, DateTime? modified, DateTime? accessed) {
        TransferFileSystem.EnsureNoLinkTraversal(_root, path);
        if (Directory.Exists(path)) {
            if (modified.HasValue) { Directory.SetLastWriteTimeUtc(path, modified.Value); }
            if (accessed.HasValue) { Directory.SetLastAccessTimeUtc(path, accessed.Value); }
        } else {
            if (modified.HasValue) { File.SetLastWriteTimeUtc(path, modified.Value); }
            if (accessed.HasValue) { File.SetLastAccessTimeUtc(path, accessed.Value); }
        }
    }

    private static bool ValidMode(string value) {
        foreach (char character in value) { if (character < '0' || character > '7') { return false; } }
        return true;
    }

    private static DateTime ParseTime(string secondsText, string microsText) {
        if (!long.TryParse(secondsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds)
            || !int.TryParse(microsText, NumberStyles.None, CultureInfo.InvariantCulture, out int micros) || micros > 999999) {
            throw new InvalidDataException("Invalid SCP timestamp.");
        }
        return DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(micros * 10L).UtcDateTime;
    }

    private async Task<string> ReadHeaderAsync(CancellationToken cancellationToken) {
        using MemoryStream header = new();
        while (true) {
            int value = await ReadByteAsync(cancellationToken).ConfigureAwait(false);
            if (value == '\n') { return HeaderEncoding.GetString(header.ToArray()); }
            if (header.Length >= 8192 || value == 0) { throw new InvalidDataException("Invalid or oversized SCP record."); }
            if (header.Length == 0 && (value == 1 || value == 2)) { throw new IOException("The SCP server rejected the transfer."); }
            header.WriteByte((byte)value);
        }
    }

    private async Task<int> ReadByteAsync(CancellationToken cancellationToken) {
        int read = await _records.ReadAsync(_singleByte, 0, 1, cancellationToken).ConfigureAwait(false);
        if (read == 0) { throw new EndOfStreamException("The SCP server ended an incomplete transfer."); }
        return _singleByte[0];
    }

    private async Task AcknowledgeAsync(CancellationToken cancellationToken) {
        _singleByte[0] = 0;
        await _acknowledgements.WriteAsync(_singleByte, 0, 1, cancellationToken).ConfigureAwait(false);
        await _acknowledgements.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private sealed class RecordContentStream : Stream {
        private readonly ScpDirectoryReceiver _owner;
        private readonly string _path;
        private readonly long _length;
        private long _position;
        private bool _ended;
        internal RecordContentStream(ScpDirectoryReceiver owner, string path, long length) { _owner = owner; _path = path; _length = length; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get => _position; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            if (count == 0) { return 0; }
            if (_position == _length) {
                if (!_ended && await _owner.ReadByteAsync(cancellationToken).ConfigureAwait(false) != 0) {
                    throw new IOException("The SCP server did not confirm the complete file content.");
                }
                _ended = true;
                return 0;
            }
            int read = await _owner._records.ReadAsync(buffer, offset, (int)Math.Min(count, _length - _position), cancellationToken).ConfigureAwait(false);
            if (read == 0) { throw new EndOfStreamException("The SCP file ended before its advertised length."); }
            _position += read;
            _owner._progress?.Invoke(_path, _position, _length);
            return read;
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
