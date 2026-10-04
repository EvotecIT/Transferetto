using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

internal sealed class TransferBatchCheckpointStore {
    private const long MaxCheckpointBytes = 16 * 1024 * 1024;
    private readonly string _path;
    private readonly object _gate = new();
    private readonly Dictionary<int, Entry> _entries;

    internal TransferBatchCheckpointStore(string path) {
        if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentException("A checkpoint path is required.", nameof(path)); }
        _path = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        TransferFileSystem.EnsureNoLinkTraversal(directory, _path);
        if (!File.Exists(_path)) { _entries = new Dictionary<int, Entry>(); return; }
        using FileStream input = new(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (input.Length > MaxCheckpointBytes) { throw new InvalidDataException("The transfer checkpoint is too large."); }
        State state = (State?)new DataContractJsonSerializer(typeof(State)).ReadObject(input)
            ?? throw new InvalidDataException("The transfer checkpoint is empty.");
        if (state.Format != 2 || state.Items == null) { throw new InvalidDataException("Unsupported transfer checkpoint format."); }
        _entries = state.Items.ToDictionary(item => item.Index);
    }

    internal async Task<bool> TryResumeAsync(int index, TransferBatchItem item, CancellationToken cancellationToken) {
        Entry? entry;
        lock (_gate) { _entries.TryGetValue(index, out entry); }
        if (entry == null || !MatchesRequest(entry, item) || entry.Sha256?.Length != 64) { return false; }
        TransferItem? source = await item.Source.GetItemAsync(item.SourcePath, cancellationToken).ConfigureAwait(false);
        TransferItem? destination = await item.Destination.GetItemAsync(item.DestinationPath, cancellationToken).ConfigureAwait(false);
        if (!MatchesItem(source, entry.SourceLength, entry.SourceModified, entry.SourceETag, entry.SourceVersion)
            || !MatchesItem(destination, entry.DestinationLength, entry.DestinationModified,
                entry.DestinationETag, entry.DestinationVersion)) { return false; }
        if (source!.ETag == null && source.VersionId == null
            && !string.Equals(await HashAsync(item.Source, item.SourcePath, source.Length, cancellationToken).ConfigureAwait(false),
                entry.Sha256, StringComparison.Ordinal)) { return false; }
        return string.Equals(await HashAsync(item.Destination, item.DestinationPath, destination!.Length, cancellationToken)
            .ConfigureAwait(false), entry.Sha256, StringComparison.Ordinal);
    }

    internal async Task RecordAsync(int index, TransferBatchItem item, TransferReceipt receipt,
        CancellationToken cancellationToken) {
        TransferItem source = await item.Source.GetItemAsync(item.SourcePath, cancellationToken).ConfigureAwait(false)
            ?? throw new IOException("The completed source item disappeared before checkpointing.");
        TransferItem destination = await item.Destination.GetItemAsync(item.DestinationPath, cancellationToken).ConfigureAwait(false)
            ?? throw new IOException("The completed destination item disappeared before checkpointing.");
        if ((source.Length.HasValue && source.Length != receipt.BytesTransferred)
            || (destination.Length.HasValue && destination.Length != receipt.BytesTransferred)
            || (receipt.SourceETag != null && !string.Equals(receipt.SourceETag, source.ETag, StringComparison.Ordinal))
            || (receipt.DestinationETag != null && !string.Equals(receipt.DestinationETag, destination.ETag, StringComparison.Ordinal))) {
            throw new IOException("An item changed before its transfer checkpoint could be saved.");
        }
        string digest = receipt.Sha256 ?? await HashAsync(item.Source, item.SourcePath, source.Length, cancellationToken)
            .ConfigureAwait(false);
        if (receipt.Sha256 != null && source.ETag == null && source.VersionId == null
            && !string.Equals(await HashAsync(item.Source, item.SourcePath, source.Length, cancellationToken).ConfigureAwait(false),
                digest, StringComparison.Ordinal)) {
            throw new IOException("The source changed before its transfer checkpoint could be saved.");
        }
        if (!string.Equals(await HashAsync(item.Destination, item.DestinationPath, destination.Length, cancellationToken)
            .ConfigureAwait(false), digest, StringComparison.Ordinal)) {
            throw new IOException("The destination changed before its transfer checkpoint could be saved.");
        }
        Entry entry = new() {
            Index = index,
            SourceEndpoint = item.Source.DisplayName,
            SourcePath = item.SourcePath,
            DestinationEndpoint = item.Destination.DisplayName,
            DestinationPath = item.DestinationPath,
            Sha256 = digest,
            PolicyFingerprint = PolicyFingerprint(item.Options),
            SourceLength = source.Length,
            SourceModified = Stamp(source),
            SourceETag = source.ETag,
            SourceVersion = source.VersionId,
            DestinationLength = destination.Length,
            DestinationModified = Stamp(destination),
            DestinationETag = destination.ETag,
            DestinationVersion = destination.VersionId
        };
        lock (_gate) {
            _entries[index] = entry;
            Save();
        }
    }

    private void Save() {
        string staged = _path + ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (FileStream output = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                TransferFileSystem.PreserveStagingPermissions(staged, _path);
                new DataContractJsonSerializer(typeof(State)).WriteObject(output,
                    new State { Format = 2, Items = _entries.Values.OrderBy(item => item.Index).ToList() });
                output.Flush(flushToDisk: true);
                if (output.Length > MaxCheckpointBytes) { throw new InvalidDataException("The transfer checkpoint is too large."); }
            }
            TransferFileSystem.CommitStagedFile(staged, _path);
        } finally {
            if (File.Exists(staged)) { File.Delete(staged); }
        }
    }

    private static bool MatchesRequest(Entry entry, TransferBatchItem item) =>
        string.Equals(entry.SourceEndpoint, item.Source.DisplayName, StringComparison.Ordinal)
        && string.Equals(entry.SourcePath, item.SourcePath, StringComparison.Ordinal)
        && string.Equals(entry.DestinationEndpoint, item.Destination.DisplayName, StringComparison.Ordinal)
        && string.Equals(entry.DestinationPath, item.DestinationPath, StringComparison.Ordinal)
        && string.Equals(entry.PolicyFingerprint, PolicyFingerprint(item.Options), StringComparison.Ordinal);

    private static string PolicyFingerprint(TransferCopyOptions? options) {
        TransferWriteOptions write = options?.WriteOptions ?? new TransferWriteOptions();
        using MemoryStream content = new();
        using (BinaryWriter writer = new(content, System.Text.Encoding.UTF8, leaveOpen: true)) {
            writer.Write((options?.ExpectedSha256 ?? string.Empty).ToLowerInvariant());
            writer.Write((int)write.Mode);
            writer.Write(write.ContentType ?? string.Empty);
            writer.Write(write.Metadata.Count);
            foreach (KeyValuePair<string, string> pair in write.Metadata.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)) {
                writer.Write(pair.Key.ToLowerInvariant());
                writer.Write(pair.Value ?? string.Empty);
            }
        }
        using SHA256 sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(content.ToArray())).Replace("-", string.Empty).ToLowerInvariant();
    }

    private static bool MatchesItem(TransferItem? item, long? length, string? modified, string? eTag,
        string? version) => item != null
        && (!length.HasValue || length == item.Length)
        && (eTag == null || string.Equals(eTag, item.ETag, StringComparison.Ordinal))
        && (version == null || string.Equals(version, item.VersionId, StringComparison.Ordinal))
        && ((eTag != null || version != null) || string.Equals(modified, Stamp(item), StringComparison.Ordinal));

    private static string? Stamp(TransferItem item) => item.LastModifiedUtc?.ToString("O", CultureInfo.InvariantCulture);

    private static async Task<string> HashAsync(ITransferEndpoint endpoint, string path, long? expectedLength,
        CancellationToken cancellationToken) {
        using TransferReadHandle handle = await endpoint.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);
        using SHA256 sha = SHA256.Create();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
        long count = 0;
        try {
            while (true) {
                int read = await handle.Stream.ReadAsync(buffer, 0, 81920, cancellationToken).ConfigureAwait(false);
                if (read == 0) { break; }
                count = checked(count + read);
                sha.TransformBlock(buffer, 0, read, null, 0);
            }
            if (expectedLength.HasValue && count != expectedLength.Value) {
                throw new IOException("An item changed while verifying its transfer checkpoint.");
            }
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(sha.Hash!).Replace("-", string.Empty).ToLowerInvariant();
        } finally {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    [DataContract]
    private sealed class State {
        [DataMember(Order = 1)] public int Format { get; set; }
        [DataMember(Order = 2)] public List<Entry>? Items { get; set; }
    }

    [DataContract]
    private sealed class Entry {
        [DataMember(Order = 1)] public int Index { get; set; }
        [DataMember(Order = 2)] public string? SourceEndpoint { get; set; }
        [DataMember(Order = 3)] public string? SourcePath { get; set; }
        [DataMember(Order = 4)] public string? DestinationEndpoint { get; set; }
        [DataMember(Order = 5)] public string? DestinationPath { get; set; }
        [DataMember(Order = 6)] public string? Sha256 { get; set; }
        [DataMember(Order = 7)] public long? SourceLength { get; set; }
        [DataMember(Order = 8)] public string? SourceModified { get; set; }
        [DataMember(Order = 9)] public string? SourceETag { get; set; }
        [DataMember(Order = 10)] public string? SourceVersion { get; set; }
        [DataMember(Order = 11)] public long? DestinationLength { get; set; }
        [DataMember(Order = 12)] public string? DestinationModified { get; set; }
        [DataMember(Order = 13)] public string? DestinationETag { get; set; }
        [DataMember(Order = 14)] public string? DestinationVersion { get; set; }
        [DataMember(Order = 15)] public string? PolicyFingerprint { get; set; }
    }
}
