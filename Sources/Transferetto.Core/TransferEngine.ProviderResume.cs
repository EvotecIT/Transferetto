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

public static partial class TransferEngine {
    /// <summary>Resumes an identity-checked ranged copy into a local file or staged cloud upload.</summary>
    /// <remarks>Store the checkpoint in a private directory. Cancellation preserves staged content and the
    /// checkpoint; a changed source or incompatible checkpoint fails without committing it.</remarks>
    public static Task<TransferReceipt> CopyResumableAsync(ITransferEndpoint source, string sourcePath,
        ITransferEndpoint destination, string destinationPath, TransferResumeOptions options,
        CancellationToken cancellationToken = default) {
        if (source == null) { throw new ArgumentNullException(nameof(source)); }
        if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
        if (destination is FileSystemTransferEndpoint file) {
            return CopyResumableToFileAsync(source, sourcePath, file, destinationPath, options, cancellationToken);
        }
        if (destination is not ITransferResumableWriteEndpoint staged) {
            throw new NotSupportedException("The destination does not support staged resumable writes.");
        }
        return CopyResumableToProviderAsync(source, sourcePath, destination, staged, destinationPath,
            options, cancellationToken);
    }

    private static async Task<TransferReceipt> CopyResumableToProviderAsync(ITransferEndpoint source,
        string sourcePath, ITransferEndpoint destination, ITransferResumableWriteEndpoint staged,
        string destinationPath, TransferResumeOptions options, CancellationToken cancellationToken) {
        if (source == null) { throw new ArgumentNullException(nameof(source)); }
        if (source is not ITransferRangeEndpoint ranged) {
            throw new NotSupportedException("The source endpoint does not support identity-checked ranged reads.");
        }
        if (string.IsNullOrEmpty(sourcePath)) { throw new ArgumentException("A source path is required.", nameof(sourcePath)); }
        if (string.IsNullOrEmpty(destinationPath)) { throw new ArgumentException("A destination path is required.", nameof(destinationPath)); }
        if (options == null || string.IsNullOrWhiteSpace(options.CheckpointPath)) {
            throw new ArgumentException("A checkpoint path is required.", nameof(options));
        }
        if (options.ChunkBytes < 5 * 1024 * 1024 || options.ChunkBytes > 64 * 1024 * 1024) {
            throw new ArgumentOutOfRangeException(nameof(options), "Cloud parts must be between 5 MiB and 64 MiB.");
        }
        TransferCopyOptions copy = options.CopyOptions ?? new TransferCopyOptions();
        string? expectedHash = NormalizeExpectedHash(copy.ExpectedSha256);
        if (copy.VerifyDestination && (destination.Capabilities & TransferEndpointCapabilities.Read) == 0) {
            throw new NotSupportedException("Destination verification requires a readable endpoint.");
        }
        DateTimeOffset started = DateTimeOffset.UtcNow;
        Guid correlationId = Guid.NewGuid();
        using TransferDiagnostics.Operation operation = new(source, destination, correlationId);
        try {
            using TransferEndpointLease lease = await TransferEndpointLease.AcquireAsync(source, destination,
                cancellationToken).ConfigureAwait(false);
            TransferItem sourceItem = await source.GetItemAsync(sourcePath, cancellationToken).ConfigureAwait(false)
                ?? throw new FileNotFoundException("The source item does not exist.", sourcePath);
            if (sourceItem.Length is not long length || length < 0) {
                throw new NotSupportedException("Resumable uploads require a known source length.");
            }
            string checkpoint = Path.GetFullPath(options.CheckpointPath);
            EnsureCheckpointNotItemPath(checkpoint, source, sourcePath);
            EnsureCheckpointNotItemPath(checkpoint, destination, destinationPath);
            if (length == 0) {
                if (File.Exists(checkpoint)) {
                    throw new IOException("A resume checkpoint cannot be reused for an empty source item.");
                }
                return await CopyAsync(source, sourcePath, destination, destinationPath, copy,
                    cancellationToken).ConfigureAwait(false);
            }
            int maxParts = destination.Scheme == "s3" ? 10000 : 50000;
            if (1 + (length - 1) / options.ChunkBytes > maxParts) {
                throw new ArgumentOutOfRangeException(nameof(options), "The transfer exceeds the provider part limit.");
            }
            string directory = Path.GetDirectoryName(checkpoint)!;
            Directory.CreateDirectory(directory);
            TransferFileSystem.EnsureNoLinkTraversal(directory, checkpoint);
            TransferWriteOptions write = CloneWriteOptions(copy.WriteOptions, sourceItem,
                destination.Capabilities);
            ProviderResumeState state;
            if (File.Exists(checkpoint)) {
                state = ReadProviderResumeState(checkpoint);
                ValidateProviderResumeState(state, source, sourcePath, sourceItem, staged,
                    destinationPath, options.ChunkBytes, write);
            } else {
                TransferItem? existing = write.Mode == TransferWriteMode.Overwrite ? null
                    : await destination.GetItemAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                if (existing != null) {
                    if (write.Mode == TransferWriteMode.SkipIfExists) {
                        TransferReceipt skipped = ProviderReceipt(source, sourcePath, destination, destinationPath,
                            sourceItem, 0, null, existing.ETag, correlationId, started, false,
                            TransferReceiptOutcome.Skipped);
                        operation.Complete(skipped);
                        return skipped;
                    }
                    throw new IOException("The destination already exists.");
                }
                string session = await staged.BeginResumableWriteAsync(destinationPath, write,
                    cancellationToken).ConfigureAwait(false);
                state = ProviderResumeState.Create(source, sourcePath, sourceItem, staged, destinationPath,
                    options.ChunkBytes, write, session);
                WriteProviderResumeState(checkpoint, state);
            }
            if (state.FullSha256 != null) {
                TransferItem? committed = await destination.GetItemAsync(destinationPath, cancellationToken)
                    .ConfigureAwait(false);
                if (committed?.Length == length && await DestinationMatchesAsync(destination, destinationPath,
                    state.FullSha256, length, cancellationToken).ConfigureAwait(false)) {
                    File.Delete(checkpoint);
                    TransferReceipt recovered = ProviderReceipt(source, sourcePath, destination, destinationPath,
                        sourceItem, length, state.FullSha256, committed.ETag, correlationId, started, true,
                        TransferReceiptOutcome.Copied);
                    operation.Complete(recovered);
                    return recovered;
                }
            }
            IReadOnlyList<TransferResumablePart> remote;
            try {
                remote = await staged.ListResumablePartsAsync(destinationPath, state.SessionId,
                    cancellationToken).ConfigureAwait(false);
            } catch (Exception exception) when (exception is not OperationCanceledException && state.FullSha256 != null) {
                // Completion may have succeeded just before a crash. Only accept the committed object after
                // checking its content, never from the absence of the upload session alone.
                TransferItem? candidate = await destination.GetItemAsync(destinationPath, cancellationToken)
                    .ConfigureAwait(false);
                if (candidate?.Length != length || !await DestinationMatchesAsync(destination, destinationPath,
                    state.FullSha256, length, cancellationToken).ConfigureAwait(false)) { throw; }
                File.Delete(checkpoint);
                TransferReceipt recovered = ProviderReceipt(source, sourcePath, destination, destinationPath,
                    sourceItem, length, state.FullSha256, candidate.ETag, correlationId, started, true,
                    TransferReceiptOutcome.Copied);
                operation.Complete(recovered);
                return recovered;
            }
            Dictionary<int, TransferResumablePart> serviceParts = remote.ToDictionary(part => part.Number);
            long offset = 0;
            int count = checked((int)(1 + (length - 1) / options.ChunkBytes));
            for (int number = 1; number <= count; number++) {
                cancellationToken.ThrowIfCancellationRequested();
                long partLength = Math.Min(options.ChunkBytes, length - offset);
                ProviderResumePart? saved = state.Parts.FirstOrDefault(part => part.Number == number);
                if (saved != null && saved.Length == partLength && saved.Sha256?.Length == 64
                    && serviceParts.TryGetValue(number, out TransferResumablePart? service)
                    && service.Length == partLength && service.Token == saved.Token) {
                    offset += partLength;
                    copy.Progress?.Report(new TransferProgress { SourcePath = sourcePath,
                        DestinationPath = destinationPath, BytesTransferred = offset, TotalBytes = length });
                    continue;
                }
                using TransferReadHandle range = await ranged.OpenReadRangeAsync(sourcePath, offset,
                    partLength, sourceItem, cancellationToken).ConfigureAwait(false);
                using MemoryStream buffer = new();
                await TransferContent.CopyToAsync(range.Stream, buffer, partLength, cancellationToken)
                    .ConfigureAwait(false);
                string digest;
                using (SHA256 sha = SHA256.Create()) {
                    digest = Hex(sha.ComputeHash(buffer.GetBuffer(), 0, checked((int)buffer.Length)));
                }
                buffer.Position = 0;
                TransferResumablePart uploaded = await staged.WriteResumablePartAsync(destinationPath,
                    state.SessionId, number, buffer, partLength, cancellationToken).ConfigureAwait(false);
                if (uploaded.Number != number || uploaded.Length != partLength || string.IsNullOrEmpty(uploaded.Token)) {
                    throw new IOException("The provider returned an invalid staged part.");
                }
                state.Parts.RemoveAll(part => part.Number == number);
                state.Parts.Add(new ProviderResumePart { Number = number, Length = partLength,
                    Token = uploaded.Token, Sha256 = digest });
                WriteProviderResumeState(checkpoint, state);
                offset += partLength;
                copy.Progress?.Report(new TransferProgress { SourcePath = sourcePath,
                    DestinationPath = destinationPath, BytesTransferred = offset, TotalBytes = length });
            }
            string sourceHash = await VerifySourceAgainstPartsAsync(ranged, source, sourcePath, sourceItem,
                state, cancellationToken).ConfigureAwait(false);
            if (expectedHash != null && sourceHash != expectedHash) {
                throw new InvalidDataException("The source content does not match the expected SHA-256 digest.");
            }
            state.FullSha256 = sourceHash;
            WriteProviderResumeState(checkpoint, state);
            cancellationToken.ThrowIfCancellationRequested();
            TransferWriteResult result = await staged.CompleteResumableWriteAsync(destinationPath, state.SessionId,
                state.Parts.OrderBy(part => part.Number).Select(part => new TransferResumablePart {
                    Number = part.Number, Length = part.Length, Token = part.Token!
                }).ToArray(), length, write, cancellationToken).ConfigureAwait(false);
            if (!result.WasWritten) {
                await staged.AbortResumableWriteAsync(destinationPath, state.SessionId,
                    CancellationToken.None).ConfigureAwait(false);
                File.Delete(checkpoint);
                TransferReceipt skipped = ProviderReceipt(source, sourcePath, destination, destinationPath,
                    sourceItem, 0, null, result.Item.ETag, correlationId, started, false,
                    TransferReceiptOutcome.Skipped);
                operation.Complete(skipped);
                return skipped;
            }
            if (copy.VerifyDestination && !await DestinationMatchesAsync(destination, destinationPath,
                sourceHash, length, cancellationToken).ConfigureAwait(false)) {
                throw new InvalidDataException("The committed destination failed SHA-256 readback verification.");
            }
            File.Delete(checkpoint);
            TransferReceipt receipt = ProviderReceipt(source, sourcePath, destination, destinationPath,
                sourceItem, length, sourceHash, result.Item.ETag, correlationId, started,
                copy.VerifyDestination, TransferReceiptOutcome.Copied);
            operation.Complete(receipt);
            return receipt;
        } catch (Exception exception) { operation.Fail(exception); throw; }
    }

    private static async Task<string> VerifySourceAgainstPartsAsync(ITransferRangeEndpoint ranged,
        ITransferEndpoint source, string path, TransferItem expected, ProviderResumeState state,
        CancellationToken cancellationToken) {
        TransferItem current = await source.GetItemAsync(path, cancellationToken).ConfigureAwait(false)
            ?? throw new IOException("The source disappeared during a resumable copy.");
        if (!ResumeState.SameSource(expected, current)) { throw new IOException("The source changed during a resumable copy."); }
        using SHA256 overall = SHA256.Create();
        byte[] io = ArrayPool<byte>.Shared.Rent(81920);
        long offset = 0;
        try {
            foreach (ProviderResumePart part in state.Parts.OrderBy(part => part.Number)) {
                if (part.Number != offset / state.ChunkBytes + 1 || part.Length <= 0
                    || part.Length > state.ChunkBytes || part.Sha256?.Length != 64) {
                    throw new InvalidDataException("The resume checkpoint has invalid part boundaries.");
                }
                using TransferReadHandle range = await ranged.OpenReadRangeAsync(path, offset, part.Length,
                    expected, cancellationToken).ConfigureAwait(false);
                using SHA256 partSha = SHA256.Create();
                long remaining = part.Length;
                while (remaining > 0) {
                    int read = await range.Stream.ReadAsync(io, 0, (int)Math.Min(io.Length, remaining),
                        cancellationToken).ConfigureAwait(false);
                    if (read == 0) { throw new EndOfStreamException("A source range ended early."); }
                    partSha.TransformBlock(io, 0, read, null, 0);
                    overall.TransformBlock(io, 0, read, null, 0);
                    remaining -= read;
                }
                partSha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                if (Hex(partSha.Hash!) != part.Sha256) {
                    throw new IOException("The source content changed since a part was staged.");
                }
                offset += part.Length;
            }
            if (offset != expected.Length) { throw new InvalidDataException("The resume checkpoint length is inconsistent."); }
            overall.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            current = await source.GetItemAsync(path, cancellationToken).ConfigureAwait(false)
                ?? throw new IOException("The source disappeared during verification.");
            if (!ResumeState.SameSource(expected, current)) { throw new IOException("The source changed during verification."); }
            return Hex(overall.Hash!);
        } finally { ArrayPool<byte>.Shared.Return(io, clearArray: true); }
    }

    private static async Task<bool> DestinationMatchesAsync(ITransferEndpoint destination, string path,
        string expectedHash, long length, CancellationToken cancellationToken) {
        using TransferReadHandle read = await destination.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (read.Item.Length != length) { return false; }
        return string.Equals(await HashStreamAsync(read.Stream, length, cancellationToken).ConfigureAwait(false),
            expectedHash, StringComparison.Ordinal);
    }

    private static TransferReceipt ProviderReceipt(ITransferEndpoint source, string sourcePath,
        ITransferEndpoint destination, string destinationPath, TransferItem sourceItem, long bytes,
        string? hash, string? destinationETag, Guid correlationId, DateTimeOffset started, bool verified,
        TransferReceiptOutcome outcome) => new() {
            CorrelationId = correlationId, SourceEndpoint = source.DisplayName, SourcePath = sourcePath,
            DestinationEndpoint = destination.DisplayName, DestinationPath = destinationPath,
            Outcome = outcome, BytesTransferred = bytes, Sha256 = hash, SourceETag = sourceItem.ETag,
            DestinationETag = destinationETag, DestinationVerified = verified,
            StartedAtUtc = started, CompletedAtUtc = DateTimeOffset.UtcNow
        };

    private static string Hex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

    private static ProviderResumeState ReadProviderResumeState(string path) {
        using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (input.Length > 16 * 1024 * 1024) { throw new InvalidDataException("The resume checkpoint is too large."); }
        return (ProviderResumeState?)new DataContractJsonSerializer(typeof(ProviderResumeState)).ReadObject(input)
            ?? throw new InvalidDataException("The resume checkpoint is empty.");
    }

    private static void WriteProviderResumeState(string path, ProviderResumeState state) {
        string temporary = path + ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (FileStream output = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                TransferFileSystem.PreserveStagingPermissions(temporary, path);
                new DataContractJsonSerializer(typeof(ProviderResumeState)).WriteObject(output, state);
                output.Flush(flushToDisk: true);
                if (output.Length > 16 * 1024 * 1024) { throw new InvalidDataException("The resume checkpoint is too large."); }
            }
            TransferFileSystem.CommitStagedFile(temporary, path);
        } finally { if (File.Exists(temporary)) { File.Delete(temporary); } }
    }

    private static void ValidateProviderResumeState(ProviderResumeState state, ITransferEndpoint source,
        string sourcePath, TransferItem sourceItem, ITransferResumableWriteEndpoint destination,
        string destinationPath, int chunkBytes, TransferWriteOptions write) {
        if (state.Format != 1 || state.Parts == null || string.IsNullOrEmpty(state.SessionId)
            || state.SessionId.Length > 2048 || state.SourceEndpoint != SourceIdentity(source)
            || state.SourcePath != sourcePath || state.SourceLength != sourceItem.Length
            || state.SourceETag != sourceItem.ETag || state.SourceVersion != sourceItem.VersionId
            || (sourceItem.ETag == null && sourceItem.VersionId == null
                && state.SourceModified != ResumeState.Stamp(sourceItem))
            || state.DestinationEndpoint != destination.ResumableIdentity
            || state.DestinationPath != destinationPath || state.ChunkBytes != chunkBytes
            || state.WriteMode != write.Mode || state.ContentType != write.ContentType
            || state.Metadata == null || state.Metadata.Count != write.Metadata.Count
            || write.Metadata.Any(pair => !state.Metadata.TryGetValue(pair.Key, out string? value)
                || value != pair.Value)) {
            throw new IOException("The resume checkpoint does not match the source, destination, or write policy.");
        }
        if (state.Parts.Count > 50000 || state.Parts.Any(part => part.Number < 1 || part.Length <= 0
            || part.Length > chunkBytes || part.Token == null || part.Token.Length > 2048
            || part.Sha256?.Length != 64)) {
            throw new InvalidDataException("The resume checkpoint contains invalid staged parts.");
        }
    }

    private static string SourceIdentity(ITransferEndpoint endpoint) =>
        endpoint is ITransferResumableWriteEndpoint resumable ? resumable.ResumableIdentity : endpoint.DisplayName;

    private static void EnsureCheckpointNotItemPath(string checkpoint, ITransferEndpoint endpoint, string itemPath) {
        if (endpoint is not FileSystemTransferEndpoint file) { return; }
        string item = file.ResolveForResume(itemPath);
        StringComparison comparison = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (string.Equals(checkpoint, item, comparison)) {
            throw new ArgumentException("A checkpoint path must differ from every local source and destination item.", nameof(checkpoint));
        }
    }

    [DataContract]
    private sealed class ProviderResumeState {
        [DataMember(Order = 1)] public int Format { get; set; } = 1;
        [DataMember(Order = 2)] public string? SourceEndpoint { get; set; }
        [DataMember(Order = 3)] public string? SourcePath { get; set; }
        [DataMember(Order = 4)] public long? SourceLength { get; set; }
        [DataMember(Order = 5)] public string? SourceModified { get; set; }
        [DataMember(Order = 6)] public string? SourceETag { get; set; }
        [DataMember(Order = 7)] public string? SourceVersion { get; set; }
        [DataMember(Order = 8)] public string? DestinationEndpoint { get; set; }
        [DataMember(Order = 9)] public string? DestinationPath { get; set; }
        [DataMember(Order = 10)] public string SessionId { get; set; } = string.Empty;
        [DataMember(Order = 11)] public int ChunkBytes { get; set; }
        [DataMember(Order = 12)] public TransferWriteMode WriteMode { get; set; }
        [DataMember(Order = 13)] public string? ContentType { get; set; }
        [DataMember(Order = 14)] public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        [DataMember(Order = 15)] public List<ProviderResumePart> Parts { get; set; } = new();
        [DataMember(Order = 16)] public string? FullSha256 { get; set; }

        internal static ProviderResumeState Create(ITransferEndpoint source, string sourcePath,
            TransferItem sourceItem, ITransferResumableWriteEndpoint destination, string destinationPath,
            int chunkBytes, TransferWriteOptions write, string sessionId) => new() {
                SourceEndpoint = SourceIdentity(source), SourcePath = sourcePath, SourceLength = sourceItem.Length,
                SourceModified = ResumeState.Stamp(sourceItem), SourceETag = sourceItem.ETag,
                SourceVersion = sourceItem.VersionId, DestinationEndpoint = destination.ResumableIdentity,
                DestinationPath = destinationPath, SessionId = sessionId, ChunkBytes = chunkBytes,
                WriteMode = write.Mode, ContentType = write.ContentType,
                Metadata = new Dictionary<string, string>(write.Metadata, StringComparer.OrdinalIgnoreCase)
            };
    }

    [DataContract]
    private sealed class ProviderResumePart {
        [DataMember(Order = 1)] public int Number { get; set; }
        [DataMember(Order = 2)] public long Length { get; set; }
        [DataMember(Order = 3)] public string? Token { get; set; }
        [DataMember(Order = 4)] public string? Sha256 { get; set; }
    }
}
