using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

public static partial class TransferEngine {
    /// <summary>Resumes identity-checked source ranges into a staged local file, then commits it atomically.</summary>
    /// <remarks>The checkpoint and staged file survive cancellation. Keep the checkpoint directory private to
    /// the automation identity. A changed source or invalid checkpoint fails closed without replacing the target.</remarks>
    public static async Task<TransferReceipt> CopyResumableToFileAsync(ITransferEndpoint source, string sourcePath,
        FileSystemTransferEndpoint destination, string destinationPath, TransferResumeOptions options,
        CancellationToken cancellationToken = default) {
        if (source == null) { throw new ArgumentNullException(nameof(source)); }
        if (source is not ITransferRangeEndpoint ranged) {
            throw new NotSupportedException("The source endpoint does not support identity-checked ranged reads.");
        }
        if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
        if (string.IsNullOrEmpty(sourcePath)) { throw new ArgumentException("A source path is required.", nameof(sourcePath)); }
        if (string.IsNullOrEmpty(destinationPath)) { throw new ArgumentException("A destination path is required.", nameof(destinationPath)); }
        if (options == null) { throw new ArgumentNullException(nameof(options)); }
        if (options.ChunkBytes < 65536 || options.ChunkBytes > 64 * 1024 * 1024) {
            throw new ArgumentOutOfRangeException(nameof(options), "ChunkBytes must be between 64 KiB and 64 MiB.");
        }
        if (string.IsNullOrWhiteSpace(options.CheckpointPath)) {
            throw new ArgumentException("A checkpoint path is required.", nameof(options));
        }
        TransferCopyOptions copyOptions = options.CopyOptions ?? new TransferCopyOptions();
        string? expectedHash = NormalizeExpectedHash(copyOptions.ExpectedSha256);
        DateTimeOffset started = DateTimeOffset.UtcNow;
        Guid correlationId = Guid.NewGuid();
        using TransferDiagnostics.Operation operation = new(source, destination, correlationId);
        try {
            using TransferEndpointLease lease = await TransferEndpointLease.AcquireAsync(source, destination,
                cancellationToken).ConfigureAwait(false);
            TransferItem sourceItem = await source.GetItemAsync(sourcePath, cancellationToken).ConfigureAwait(false)
                ?? throw new FileNotFoundException("The source item does not exist.", sourcePath);
            if (sourceItem.Length is not long length || length < 0) {
                throw new NotSupportedException("Resumable ranged copies require a known source length.");
            }
            string target = destination.ResolveForResume(destinationPath);
            string targetDirectory = Path.GetDirectoryName(target)!;
            Directory.CreateDirectory(targetDirectory);
            string checkpoint = Path.GetFullPath(options.CheckpointPath);
            EnsureCheckpointNotItemPath(checkpoint, source, sourcePath);
            string checkpointDirectory = Path.GetDirectoryName(checkpoint)!;
            StringComparison pathComparison = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (string.Equals(checkpoint, target, pathComparison)) {
                throw new ArgumentException("The checkpoint path must differ from the destination item.", nameof(options));
            }
            Directory.CreateDirectory(checkpointDirectory);
            TransferFileSystem.EnsureNoLinkTraversal(checkpointDirectory, checkpoint);
            ResumeState state;
            if (File.Exists(checkpoint)) {
                state = ReadResumeState(checkpoint);
                ValidateResumeState(state, source, sourcePath, sourceItem, destination, destinationPath,
                    targetDirectory);
                if (!File.Exists(state.StagePath)) {
                    if (File.Exists(target) && state.CommittedBytes == length) {
                        await VerifyResumeChunksAsync(target, state, source, ranged, sourcePath, sourceItem,
                            cancellationToken).ConfigureAwait(false);
                        string recoveredHash = await HashFileAsync(target, length, cancellationToken).ConfigureAwait(false);
                        if (expectedHash != null && !string.Equals(expectedHash, recoveredHash, StringComparison.Ordinal)) {
                            throw new InvalidDataException("The recovered destination does not match the expected SHA-256 digest.");
                        }
                        File.Delete(checkpoint);
                        TransferReceipt recovered = CreateResumeReceipt(source, sourcePath, destination, destinationPath,
                            sourceItem, length, recoveredHash, correlationId, started, destinationVerified: true);
                        operation.Complete(recovered);
                        return recovered;
                    }
                    throw new IOException("The staged file named by the resume checkpoint is missing.");
                }
                await VerifyResumeChunksAsync(state.StagePath, state, source, ranged, sourcePath, sourceItem,
                    cancellationToken).ConfigureAwait(false);
                using FileStream truncate = new(state.StagePath, FileMode.Open, FileAccess.Write, FileShare.None);
                truncate.SetLength(state.CommittedBytes);
            } else {
                if (File.Exists(target)) {
                    if (copyOptions.WriteOptions.Mode == TransferWriteMode.SkipIfExists) {
                        TransferReceipt skipped = new() { CorrelationId = correlationId,
                            SourceEndpoint = source.DisplayName, SourcePath = sourcePath,
                            DestinationEndpoint = destination.DisplayName, DestinationPath = destinationPath,
                            Outcome = TransferReceiptOutcome.Skipped, StartedAtUtc = started,
                            CompletedAtUtc = DateTimeOffset.UtcNow };
                        operation.Complete(skipped);
                        return skipped;
                    }
                    if (copyOptions.WriteOptions.Mode == TransferWriteMode.FailIfExists) {
                        throw new IOException("The destination file already exists.");
                    }
                }
                string stage = target + ".transferetto-resume-" + Guid.NewGuid().ToString("N") + ".part";
                TransferFileSystem.EnsureNoLinkTraversal(targetDirectory, stage);
                using (FileStream output = new(stage, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                    TransferFileSystem.PreserveStagingPermissions(stage, target);
                }
                state = ResumeState.Create(source, sourcePath, sourceItem, destination, destinationPath, stage);
                WriteResumeState(checkpoint, state);
            }

            for (long offset = state.CommittedBytes; offset < length;) {
                cancellationToken.ThrowIfCancellationRequested();
                long count = Math.Min(options.ChunkBytes, length - offset);
                using TransferReadHandle range = await ranged.OpenReadRangeAsync(sourcePath, offset, count,
                    sourceItem, cancellationToken).ConfigureAwait(false);
                string digest = await WriteResumeChunkAsync(range.Stream, state.StagePath, offset, count,
                    cancellationToken).ConfigureAwait(false);
                state.Chunks.Add(new ResumeChunk { Offset = offset, Length = count, Sha256 = digest });
                offset += count;
                state.CommittedBytes = offset;
                WriteResumeState(checkpoint, state);
                copyOptions.Progress?.Report(new TransferProgress { SourcePath = sourcePath,
                    DestinationPath = destinationPath, BytesTransferred = offset, TotalBytes = length });
            }
            string sourceHash = await HashFileAsync(state.StagePath, length, cancellationToken).ConfigureAwait(false);
            if (expectedHash != null && !string.Equals(expectedHash, sourceHash, StringComparison.Ordinal)) {
                throw new InvalidDataException("The source content does not match the expected SHA-256 digest.");
            }
            TransferItem currentSource = await source.GetItemAsync(sourcePath, cancellationToken).ConfigureAwait(false)
                ?? throw new IOException("The source disappeared during a resumable copy.");
            if (!ResumeState.SameSource(sourceItem, currentSource)) {
                throw new IOException("The source changed during a resumable copy.");
            }
            if (sourceItem.ETag == null && sourceItem.VersionId == null) {
                using TransferReadHandle sourceVerification = await source.OpenReadAsync(sourcePath, cancellationToken)
                    .ConfigureAwait(false);
                string currentHash = await HashStreamAsync(sourceVerification.Stream, length, cancellationToken)
                    .ConfigureAwait(false);
                if (!string.Equals(currentHash, sourceHash, StringComparison.Ordinal)) {
                    throw new IOException("The source content changed during a resumable copy.");
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (copyOptions.WriteOptions.Mode != TransferWriteMode.Overwrite && File.Exists(target)) {
                if (copyOptions.WriteOptions.Mode == TransferWriteMode.FailIfExists) {
                    throw new IOException("The destination file was created during the resumable copy.");
                }
                throw new IOException("The destination file was created during the resumable copy; retry to inspect it.");
            }
            TransferFileSystem.CommitStagedFile(state.StagePath, target,
                overwrite: copyOptions.WriteOptions.Mode == TransferWriteMode.Overwrite);
            File.Delete(checkpoint);
            bool verified = copyOptions.VerifyDestination;
            if (verified && !string.Equals(await HashFileAsync(target, length, cancellationToken).ConfigureAwait(false),
                sourceHash, StringComparison.Ordinal)) {
                throw new InvalidDataException("The committed destination failed SHA-256 readback verification.");
            }
            TransferReceipt receipt = CreateResumeReceipt(source, sourcePath, destination, destinationPath,
                sourceItem, length, sourceHash, correlationId, started, verified);
            operation.Complete(receipt);
            return receipt;
        } catch (Exception exception) { operation.Fail(exception); throw; }
    }

    private static TransferReceipt CreateResumeReceipt(ITransferEndpoint source, string sourcePath,
        FileSystemTransferEndpoint destination, string destinationPath, TransferItem sourceItem, long length,
        string sha256, Guid correlationId, DateTimeOffset started, bool destinationVerified) => new() {
            CorrelationId = correlationId, SourceEndpoint = source.DisplayName, SourcePath = sourcePath,
            DestinationEndpoint = destination.DisplayName, DestinationPath = destinationPath,
            Outcome = TransferReceiptOutcome.Copied, BytesTransferred = length, Sha256 = sha256,
            DestinationVerified = destinationVerified, SourceETag = sourceItem.ETag,
            StartedAtUtc = started, CompletedAtUtc = DateTimeOffset.UtcNow
        };

    private static ResumeState ReadResumeState(string path) {
        using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (input.Length > 16 * 1024 * 1024) { throw new InvalidDataException("The resume checkpoint is too large."); }
        ResumeState state = (ResumeState?)new DataContractJsonSerializer(typeof(ResumeState)).ReadObject(input)
            ?? throw new InvalidDataException("The resume checkpoint is empty.");
        return state;
    }

    private static void WriteResumeState(string path, ResumeState state) {
        string staged = path + ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (FileStream output = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                TransferFileSystem.PreserveStagingPermissions(staged, path);
                new DataContractJsonSerializer(typeof(ResumeState)).WriteObject(output, state);
                output.Flush(flushToDisk: true);
                if (output.Length > 16 * 1024 * 1024) { throw new InvalidDataException("The resume checkpoint is too large."); }
            }
            TransferFileSystem.CommitStagedFile(staged, path);
        } finally { if (File.Exists(staged)) { File.Delete(staged); } }
    }

    private static void ValidateResumeState(ResumeState state, ITransferEndpoint source, string sourcePath,
        TransferItem sourceItem, FileSystemTransferEndpoint destination, string destinationPath,
        string targetDirectory) {
        if (state.Format != 1 || state.Chunks == null || state.StagePath == null
            || state.SourceEndpoint != source.DisplayName || state.SourcePath != sourcePath
            || state.DestinationEndpoint != destination.DisplayName || state.DestinationPath != destinationPath
            || state.SourceLength != sourceItem.Length || state.SourceETag != sourceItem.ETag
            || state.SourceVersion != sourceItem.VersionId
            || ((sourceItem.ETag == null && sourceItem.VersionId == null)
                && state.SourceModified != ResumeState.Stamp(sourceItem))) {
            throw new IOException("The resume checkpoint does not match the source or destination.");
        }
        string target = destination.ResolveForResume(destinationPath);
        string stage = Path.GetFullPath(state.StagePath);
        StringComparison comparison = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(Path.GetDirectoryName(stage), targetDirectory, comparison)
            || !Path.GetFileName(stage).StartsWith(Path.GetFileName(target) + ".transferetto-resume-", comparison)
            || !stage.EndsWith(".part", StringComparison.Ordinal)) {
            throw new InvalidDataException("The resume checkpoint contains an invalid staged path.");
        }
        TransferFileSystem.EnsureNoLinkTraversal(targetDirectory, stage);
    }

    private static async Task VerifyResumeChunksAsync(string path, ResumeState state, ITransferEndpoint source,
        ITransferRangeEndpoint ranged, string sourcePath, TransferItem sourceItem, CancellationToken cancellationToken) {
        using FileStream staged = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        long expectedOffset = 0;
        foreach (ResumeChunk chunk in state.Chunks) {
            if (chunk.Offset != expectedOffset || chunk.Length <= 0 || chunk.Length > 64L * 1024 * 1024
                || chunk.Sha256?.Length != 64 || staged.Length < expectedOffset + chunk.Length) {
                throw new InvalidDataException("The resume checkpoint has invalid chunk boundaries.");
            }
            string stagedHash = await HashRangeAsync(staged, chunk.Offset, chunk.Length, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(stagedHash, chunk.Sha256, StringComparison.Ordinal)) {
                throw new InvalidDataException("A staged resume chunk failed SHA-256 verification.");
            }
            if (sourceItem.ETag == null && sourceItem.VersionId == null) {
                using TransferReadHandle range = await ranged.OpenReadRangeAsync(sourcePath, chunk.Offset, chunk.Length,
                    sourceItem, cancellationToken).ConfigureAwait(false);
                string sourceHash = await HashStreamAsync(range.Stream, chunk.Length, cancellationToken).ConfigureAwait(false);
                if (!string.Equals(sourceHash, chunk.Sha256, StringComparison.Ordinal)) {
                    throw new IOException("The source content changed since the resume checkpoint.");
                }
            }
            expectedOffset += chunk.Length;
        }
        if (expectedOffset != state.CommittedBytes || expectedOffset > sourceItem.Length) {
            throw new InvalidDataException("The resume checkpoint byte count is inconsistent.");
        }
    }

    private static async Task<string> WriteResumeChunkAsync(Stream source, string stage, long offset, long length,
        CancellationToken cancellationToken) {
        using FileStream output = new(stage, FileMode.Open, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        output.Seek(offset, SeekOrigin.Begin);
        using SHA256 sha = SHA256.Create();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
        long remaining = length;
        try {
            while (remaining > 0) {
                int read = await source.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0) { throw new EndOfStreamException("A source range ended before its expected length."); }
                sha.TransformBlock(buffer, 0, read, null, 0);
                await output.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
                remaining -= read;
            }
            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
            output.Flush(flushToDisk: true);
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(sha.Hash!).Replace("-", string.Empty).ToLowerInvariant();
        } finally { ArrayPool<byte>.Shared.Return(buffer, clearArray: true); }
    }

    private static async Task<string> HashRangeAsync(FileStream stream, long offset, long length,
        CancellationToken cancellationToken) {
        stream.Seek(offset, SeekOrigin.Begin);
        return await HashStreamAsync(stream, length, cancellationToken, requireEnd: false).ConfigureAwait(false);
    }

    private static async Task<string> HashFileAsync(string path, long length, CancellationToken cancellationToken) {
        using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
        return await HashStreamAsync(input, length, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> HashStreamAsync(Stream stream, long length, CancellationToken cancellationToken,
        bool requireEnd = true) {
        using SHA256 sha = SHA256.Create();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
        long remaining = length;
        try {
            while (remaining > 0) {
                int read = await stream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0) { throw new EndOfStreamException("Content ended before its expected length."); }
                sha.TransformBlock(buffer, 0, read, null, 0);
                remaining -= read;
            }
            if (requireEnd && await stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false) != 0) {
                throw new IOException("Content exceeded its expected length.");
            }
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(sha.Hash!).Replace("-", string.Empty).ToLowerInvariant();
        } finally {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    [DataContract]
    private sealed class ResumeState {
        [DataMember(Order = 1)] public int Format { get; set; } = 1;
        [DataMember(Order = 2)] public string? SourceEndpoint { get; set; }
        [DataMember(Order = 3)] public string? SourcePath { get; set; }
        [DataMember(Order = 4)] public long? SourceLength { get; set; }
        [DataMember(Order = 5)] public string? SourceModified { get; set; }
        [DataMember(Order = 6)] public string? SourceETag { get; set; }
        [DataMember(Order = 7)] public string? SourceVersion { get; set; }
        [DataMember(Order = 8)] public string? DestinationEndpoint { get; set; }
        [DataMember(Order = 9)] public string? DestinationPath { get; set; }
        [DataMember(Order = 10)] public string StagePath { get; set; } = string.Empty;
        [DataMember(Order = 11)] public long CommittedBytes { get; set; }
        [DataMember(Order = 12)] public System.Collections.Generic.List<ResumeChunk> Chunks { get; set; } = new();

        internal static ResumeState Create(ITransferEndpoint source, string sourcePath, TransferItem sourceItem,
            FileSystemTransferEndpoint destination, string destinationPath, string stage) => new() {
                SourceEndpoint = source.DisplayName, SourcePath = sourcePath, SourceLength = sourceItem.Length,
                SourceModified = Stamp(sourceItem), SourceETag = sourceItem.ETag, SourceVersion = sourceItem.VersionId,
                DestinationEndpoint = destination.DisplayName, DestinationPath = destinationPath, StagePath = stage
            };
        internal static bool SameSource(TransferItem first, TransferItem second) => first.Length == second.Length
            && first.ETag == second.ETag && first.VersionId == second.VersionId
            && ((first.ETag != null || first.VersionId != null) || Stamp(first) == Stamp(second));
        internal static string? Stamp(TransferItem item) => item.LastModifiedUtc?.ToString("O", CultureInfo.InvariantCulture);
    }

    [DataContract]
    private sealed class ResumeChunk {
        [DataMember(Order = 1)] public long Offset { get; set; }
        [DataMember(Order = 2)] public long Length { get; set; }
        [DataMember(Order = 3)] public string? Sha256 { get; set; }
    }
}
