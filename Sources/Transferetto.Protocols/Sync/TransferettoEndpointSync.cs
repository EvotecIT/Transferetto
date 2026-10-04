using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto;

/// <summary>Runs the established synchronization planner over any two file-oriented transfer endpoints.</summary>
/// <remarks>Endpoint listings contain files, so empty directories are not represented. Endpoint writers must
/// create needed parent directories. Mirror removes empty physical directories when the destination supports it.</remarks>
public static class TransferettoEndpointSync {
    /// <summary>Builds a read-only plan from endpoint file listings.</summary>
    public static async Task<IReadOnlyList<TransferettoSyncPlanItem>> PlanAsync(
        ITransferEndpoint source, string sourcePrefix, ITransferEndpoint destination, string destinationPrefix,
        TransferettoSyncOptions? options = null, CancellationToken cancellationToken = default) {
        (IReadOnlyList<TransferettoSyncPlanItem> plan, _, _) = await BuildPlanAsync(
            source, sourcePrefix, destination, destinationPrefix, options, cancellationToken).ConfigureAwait(false);
        return plan;
    }

    /// <summary>Synchronizes endpoint files in planner order, returning receipts and the first failure.</summary>
    /// <remarks>A failed transfer stops the plan before mirror deletes. An empty source listing cannot initiate
    /// a mirror unless <paramref name="allowEmptySourceMirror"/> is explicitly set.</remarks>
    public static async Task<TransferettoEndpointSyncResult> SyncAsync(
        ITransferEndpoint source, string sourcePrefix, ITransferEndpoint destination, string destinationPrefix,
        TransferettoSyncOptions? options = null, bool allowEmptySourceMirror = false,
        CancellationToken cancellationToken = default) {
        TransferettoSyncOptions resolvedOptions = options ?? new TransferettoSyncOptions();
        (IReadOnlyList<TransferettoSyncPlanItem> plan, IReadOnlyDictionary<string, TransferItem> sourceItems,
            IReadOnlyDictionary<string, TransferItem> destinationItems) = await BuildPlanAsync(
                source, sourcePrefix, destination, destinationPrefix, resolvedOptions, cancellationToken).ConfigureAwait(false);
        if (resolvedOptions.Mode == TransferettoSyncMode.Mirror && sourceItems.Count == 0 && !allowEmptySourceMirror) {
            throw new InvalidOperationException("An empty source listing cannot initiate a mirror without explicit opt-in.");
        }
        if (!resolvedOptions.DryRun && plan.Any(item => IsDeleteDirectory(item.Action))
            && destination is not ITransferDirectoryEndpoint) {
            throw new NotSupportedException("Mirror directory removal requires an endpoint that can delete empty directories.");
        }

        string sourceRoot = NormalizePrefix(sourcePrefix);
        string destinationRoot = NormalizePrefix(destinationPrefix);
        List<TransferettoEndpointSyncItemResult> results = new(plan.Count);
        foreach (TransferettoSyncPlanItem item in plan) {
            cancellationToken.ThrowIfCancellationRequested();
            if (!item.ChangesDestination || resolvedOptions.DryRun || item.Action == TransferettoSyncAction.CreateDirectory) {
                results.Add(new TransferettoEndpointSyncItemResult(item, null, false, null));
                continue;
            }

            try {
                string relative = item.RelativePath;
                if (item.Action == TransferettoSyncAction.UploadFile || item.Action == TransferettoSyncAction.DownloadFile) {
                    if (!sourceItems.TryGetValue(relative, out TransferItem? recorded)) {
                        throw new IOException("A planned source file is no longer present in the listing.");
                    }
                    string sourcePath = Combine(sourceRoot, relative);
                    TransferItem? current = await source.GetItemAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                    EnsureSameVersion(recorded, current, "source");
                    TransferWriteOptions write = new() {
                        Mode = resolvedOptions.OverwriteExisting ? TransferWriteMode.Overwrite : TransferWriteMode.FailIfExists
                    };
                    if (resolvedOptions.PreserveTimestamps && destination is not ITransferTimestampEndpoint
                        && (destination.Capabilities & TransferEndpointCapabilities.Metadata) != 0) {
                        write.Metadata[SyncSourceIdentityKey] = SyncSourceIdentity(recorded);
                    }
                    TransferReceipt receipt = await TransferEngine.CopyAsync(source, sourcePath,
                        destination, Combine(destinationRoot, relative), new TransferCopyOptions {
                            WriteOptions = write
                        }, cancellationToken).ConfigureAwait(false);
                    if (resolvedOptions.PreserveTimestamps && current?.LastModifiedUtc is DateTimeOffset modified
                        && destination is ITransferTimestampEndpoint timestamps
                        && receipt.Outcome == TransferReceiptOutcome.Copied) {
                        await timestamps.SetLastModifiedUtcAsync(Combine(destinationRoot, relative), modified,
                            cancellationToken).ConfigureAwait(false);
                    }
                    results.Add(new TransferettoEndpointSyncItemResult(item, receipt, false, null));
                } else if (item.Action == TransferettoSyncAction.DeleteLocalFile || item.Action == TransferettoSyncAction.DeleteRemoteFile) {
                    if (!destinationItems.TryGetValue(relative, out TransferItem? recorded)) {
                        throw new IOException("A planned destination file is no longer present in the listing.");
                    }
                    string destinationPath = Combine(destinationRoot, relative);
                    TransferItem? current = await destination.GetItemAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                    EnsureSameVersion(recorded, current, "destination");
                    bool deleted = await destination.DeleteAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                    results.Add(new TransferettoEndpointSyncItemResult(item, null, deleted, null));
                } else if (IsDeleteDirectory(item.Action)) {
                    bool deleted = destination is ITransferDirectoryEndpoint directories
                        ? await directories.DeleteEmptyDirectoryAsync(Combine(destinationRoot, relative), cancellationToken)
                            .ConfigureAwait(false)
                        : false;
                    results.Add(new TransferettoEndpointSyncItemResult(item, null, deleted, null));
                } else {
                    throw new NotSupportedException($"Unsupported endpoint sync action: {item.Action}.");
                }
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            } catch (Exception exception) {
                results.Add(new TransferettoEndpointSyncItemResult(item, null, false, exception));
                break;
            }
        }
        return new TransferettoEndpointSyncResult(plan, results);
    }

    private static async Task<(IReadOnlyList<TransferettoSyncPlanItem> Plan,
        IReadOnlyDictionary<string, TransferItem> Source, IReadOnlyDictionary<string, TransferItem> Destination)> BuildPlanAsync(
        ITransferEndpoint source, string sourcePrefix, ITransferEndpoint destination, string destinationPrefix,
        TransferettoSyncOptions? options, CancellationToken cancellationToken) {
        if (source == null) { throw new ArgumentNullException(nameof(source)); }
        if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
        if ((source.Capabilities & (TransferEndpointCapabilities.List | TransferEndpointCapabilities.Read | TransferEndpointCapabilities.Inspect))
            != (TransferEndpointCapabilities.List | TransferEndpointCapabilities.Read | TransferEndpointCapabilities.Inspect)) {
            throw new NotSupportedException("Source endpoint sync requires list, inspect, and read capabilities.");
        }
        if ((destination.Capabilities & (TransferEndpointCapabilities.List | TransferEndpointCapabilities.Write | TransferEndpointCapabilities.Inspect))
            != (TransferEndpointCapabilities.List | TransferEndpointCapabilities.Write | TransferEndpointCapabilities.Inspect)) {
            throw new NotSupportedException("Destination endpoint sync requires list, inspect, and write capabilities.");
        }
        TransferettoSyncOptions resolved = options ?? new TransferettoSyncOptions();
        if (resolved.Mode == TransferettoSyncMode.Mirror && (destination.Capabilities & TransferEndpointCapabilities.Delete) == 0) {
            throw new NotSupportedException("Mirror sync requires destination delete capability.");
        }
        string sourceRoot = NormalizePrefix(sourcePrefix);
        string destinationRoot = NormalizePrefix(destinationPrefix);
        IReadOnlyList<TransferItem> sourceListing = await source.ListAsync(sourceRoot, true, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<TransferItem> destinationListing = await destination.ListAsync(destinationRoot, true, cancellationToken).ConfigureAwait(false);
        if (resolved.PreserveTimestamps && resolved.Comparison != TransferettoSyncComparison.Size
            && resolved.Comparison != TransferettoSyncComparison.Always
            && destination is not ITransferTimestampEndpoint
            && (destination.Capabilities & TransferEndpointCapabilities.Metadata) != 0) {
            destinationListing = await ApplySyncSourceIdentitiesAsync(sourceListing, sourceRoot,
                destinationListing, destinationRoot, destination, cancellationToken).ConfigureAwait(false);
        }
        var sourceManifest = BuildManifest(sourceListing, sourceRoot);
        var destinationManifest = BuildManifest(destinationListing, destinationRoot);
        if (destination.Scheme == "file" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            HashSet<string> mappedPaths = new(StringComparer.OrdinalIgnoreCase);
            foreach (TransferettoSyncEntry entry in sourceManifest.Entries) {
                if (!mappedPaths.Add(entry.RelativePath)) {
                    throw new InvalidDataException("Distinct source paths collide on the Windows destination filesystem.");
                }
            }
        }
        TransferettoSyncOptions plannerOptions = resolved.PathComparison == TransferettoSyncPathComparison.Automatic
            && destination.Scheme == "file" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CopyWithPathComparison(resolved, TransferettoSyncPathComparison.OrdinalIgnoreCase)
            : resolved;
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            sourceManifest.Entries, destinationManifest.Entries, plannerOptions, cancellationToken);
        return (plan, sourceManifest.Items, destinationManifest.Items);
    }

    private static (IReadOnlyList<TransferettoSyncEntry> Entries, IReadOnlyDictionary<string, TransferItem> Items)
        BuildManifest(IReadOnlyList<TransferItem> listing, string root) {
        Dictionary<string, TransferItem> files = new(StringComparer.Ordinal);
        HashSet<string> directories = new(StringComparer.Ordinal);
        foreach (TransferItem item in listing) {
            if (!TryRelative(root, item.Path, out string? relative)) { continue; }
            ValidateRelative(relative);
            if (files.ContainsKey(relative)) {
                throw new InvalidDataException($"The endpoint returned duplicate file paths: {relative}");
            }
            files.Add(relative, item);
            for (int slash = relative.IndexOf('/'); slash >= 0; slash = relative.IndexOf('/', slash + 1)) {
                directories.Add(relative.Substring(0, slash));
            }
        }
        if (directories.Overlaps(files.Keys)) {
            throw new InvalidDataException("The endpoint listed a file that is also a parent directory.");
        }
        List<TransferettoSyncEntry> entries = new(files.Count + directories.Count);
        entries.AddRange(directories.Select(path => new TransferettoSyncEntry { RelativePath = path, IsDirectory = true }));
        entries.AddRange(files.Select(pair => new TransferettoSyncEntry {
            RelativePath = pair.Key, Length = pair.Value.Length,
            LastWriteTimeUtc = pair.Value.LastModifiedUtc?.UtcDateTime
        }));
        return (entries, files);
    }

    private static bool TryRelative(string root, string path, out string relative) {
        relative = string.Empty;
        if (string.IsNullOrEmpty(path)) { throw new InvalidDataException("An endpoint listed an empty file path."); }
        string normalized = path.Replace('\\', '/').TrimStart('/');
        if (root.Length > 0) {
            if (!normalized.StartsWith(root + "/", StringComparison.Ordinal)) { return false; }
            normalized = normalized.Substring(root.Length + 1);
        }
        relative = normalized;
        return true;
    }

    private static string NormalizePrefix(string prefix) {
        string normalized = (prefix ?? string.Empty).Replace('\\', '/').Trim('/');
        if (normalized.Length > 0) { ValidateRelative(normalized); }
        return normalized;
    }

    private static void ValidateRelative(string path) {
        if (path.Length == 0 || path.Split('/').Any(segment => segment.Length == 0 || segment == "." || segment == "..")) {
            throw new InvalidDataException($"An endpoint listed an unsafe relative path: {path}");
        }
    }

    private static string Combine(string root, string relative) => root.Length == 0 ? relative : root + "/" + relative;

    private static bool IsDeleteDirectory(TransferettoSyncAction action) =>
        action == TransferettoSyncAction.DeleteLocalDirectory || action == TransferettoSyncAction.DeleteRemoteDirectory;

    private const string SyncSourceIdentityKey = "transferetto_sync_source";

    private static string SyncSourceIdentity(TransferItem item) {
        string value = string.Join("\n", new[] {
            item.Length?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            item.LastModifiedUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            item.ETag ?? string.Empty,
            item.VersionId ?? string.Empty
        });
        using SHA256 sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    private static async Task<IReadOnlyList<TransferItem>> ApplySyncSourceIdentitiesAsync(
        IReadOnlyList<TransferItem> sourceListing, string sourceRoot,
        IReadOnlyList<TransferItem> destinationListing, string destinationRoot,
        ITransferEndpoint destination, CancellationToken cancellationToken) {
        Dictionary<string, TransferItem> sources = new(StringComparer.Ordinal);
        foreach (TransferItem item in sourceListing) {
            if (TryRelative(sourceRoot, item.Path, out string? relative)) { sources[relative] = item; }
        }
        List<TransferItem> adjusted = new(destinationListing.Count);
        foreach (TransferItem item in destinationListing) {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryRelative(destinationRoot, item.Path, out string? relative)
                || !sources.TryGetValue(relative, out TransferItem? source)
                || !source.LastModifiedUtc.HasValue || source.Length != item.Length
                || source.LastModifiedUtc == item.LastModifiedUtc) {
                adjusted.Add(item);
                continue;
            }
            TransferItem? inspected = await destination.GetItemAsync(item.Path, cancellationToken).ConfigureAwait(false);
            if (inspected != null && inspected.Metadata.TryGetValue(SyncSourceIdentityKey, out string? stamp)
                && string.Equals(stamp, SyncSourceIdentity(source), StringComparison.Ordinal)) {
                adjusted.Add(new TransferItem {
                    Path = item.Path, Length = item.Length, LastModifiedUtc = source.LastModifiedUtc,
                    ETag = item.ETag, VersionId = item.VersionId,
                    ContentType = item.ContentType, Metadata = inspected.Metadata
                });
            } else {
                adjusted.Add(item);
            }
        }
        return adjusted;
    }

    private static TransferettoSyncOptions CopyWithPathComparison(TransferettoSyncOptions options,
        TransferettoSyncPathComparison comparison) => new() {
        PathComparison = comparison,
        Direction = options.Direction,
        Mode = options.Mode,
        Comparison = options.Comparison,
        DryRun = options.DryRun,
        OverwriteExisting = options.OverwriteExisting,
        CreateDestinationDirectories = options.CreateDestinationDirectories,
        PreserveTimestamps = options.PreserveTimestamps,
        TimestampTolerance = options.TimestampTolerance,
        IncludePatterns = options.IncludePatterns,
        ExcludePatterns = options.ExcludePatterns
    };

    private static void EnsureSameVersion(TransferItem recorded, TransferItem? current, string side) {
        if (current == null || (recorded.Length.HasValue && recorded.Length != current.Length)
            || (recorded.ETag != null && !string.Equals(recorded.ETag, current.ETag, StringComparison.Ordinal))
            || (recorded.VersionId != null && !string.Equals(recorded.VersionId, current.VersionId, StringComparison.Ordinal))
            || (recorded.ETag == null && recorded.VersionId == null && recorded.LastModifiedUtc.HasValue
                && recorded.LastModifiedUtc != current.LastModifiedUtc)) {
            throw new IOException($"The planned {side} item changed after listing; synchronization stopped.");
        }
    }
}

/// <summary>Records the plan and the actions actually attempted by endpoint synchronization.</summary>
public sealed class TransferettoEndpointSyncResult {
    internal TransferettoEndpointSyncResult(IReadOnlyList<TransferettoSyncPlanItem> plan,
        IReadOnlyList<TransferettoEndpointSyncItemResult> items) { Plan = plan; Items = items; }
    /// <summary>Gets the complete plan.</summary>
    public IReadOnlyList<TransferettoSyncPlanItem> Plan { get; }
    /// <summary>Gets attempted actions in plan order.</summary>
    public IReadOnlyList<TransferettoEndpointSyncItemResult> Items { get; }
    /// <summary>Gets whether every planned action was considered without failure.</summary>
    public bool IsSuccess => Items.Count == Plan.Count && Items.All(item => item.Error == null);
}

/// <summary>Records one endpoint synchronization action.</summary>
public sealed class TransferettoEndpointSyncItemResult {
    internal TransferettoEndpointSyncItemResult(TransferettoSyncPlanItem planItem, TransferReceipt? receipt,
        bool wasDeleted, Exception? error) { PlanItem = planItem; Receipt = receipt; WasDeleted = wasDeleted; Error = error; }
    /// <summary>Gets the planner decision.</summary>
    public TransferettoSyncPlanItem PlanItem { get; }
    /// <summary>Gets the copy receipt, if this action copied a file.</summary>
    public TransferReceipt? Receipt { get; }
    /// <summary>Gets whether the action deleted a destination file.</summary>
    public bool WasDeleted { get; }
    /// <summary>Gets a failure that stopped execution, if any.</summary>
    public Exception? Error { get; }
}
