using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace Transferetto;

/// <summary>
/// Builds protocol-neutral synchronization plans from source and destination manifests.
/// </summary>
public static partial class TransferettoSyncPlanner {
    /// <summary>
    /// Compares source and destination manifests and returns the ordered actions needed to synchronize the destination.
    /// </summary>
    public static IReadOnlyList<TransferettoSyncPlanItem> Plan(
        IEnumerable<TransferettoSyncEntry> sourceEntries,
        IEnumerable<TransferettoSyncEntry> destinationEntries,
        TransferettoSyncOptions? options = null) {
        return Plan(sourceEntries, destinationEntries, options, CancellationToken.None);
    }

    /// <summary>Builds a plan with cancellation during manifest processing and action generation.</summary>
    public static IReadOnlyList<TransferettoSyncPlanItem> Plan(
        IEnumerable<TransferettoSyncEntry> sourceEntries,
        IEnumerable<TransferettoSyncEntry> destinationEntries,
        TransferettoSyncOptions? options,
        CancellationToken cancellationToken) {
        if (sourceEntries == null) { throw new ArgumentNullException(nameof(sourceEntries)); }
        if (destinationEntries == null) { throw new ArgumentNullException(nameof(destinationEntries)); }
        TransferettoSyncOptions resolvedOptions = options ?? new TransferettoSyncOptions();
        PlannerContext context = new(resolvedOptions, cancellationToken);
        Dictionary<string, TransferettoSyncEntry> source = context.BuildManifest(sourceEntries);
        Dictionary<string, TransferettoSyncEntry> destination = context.BuildManifest(destinationEntries);
        HashSet<string> includedSourceFiles = new(
            source.Values
                .Where(entry => !entry.IsDirectory && context.IsIncluded(entry.RelativePath))
                .Select(entry => NormalizeRelativePath(entry.RelativePath)),
            context.Comparer);
        HashSet<string> includedAncestors = context.GetAncestors(includedSourceFiles);
        HashSet<string> blockedDirectories = new(context.Comparer);

        List<TransferettoSyncPlanItem> plan = new();
        List<(TransferettoSyncEntry Source, TransferettoSyncEntry Destination, TransferettoSyncAction TransferAction)> directoryReplacementTransfers = new();
        foreach (TransferettoSyncEntry sourceDirectory in source.Values
                     .Where(entry => entry.IsDirectory && (context.IsIncluded(entry.RelativePath) || includedAncestors.Contains(NormalizeRelativePath(entry.RelativePath))))
                     .OrderBy(entry => entry.RelativePath.Count(static character => character == '/'))
                     .ThenBy(entry => entry.RelativePath, StringComparer.Ordinal)) {
            string relativePath = NormalizeRelativePath(sourceDirectory.RelativePath);
            cancellationToken.ThrowIfCancellationRequested();
            destination.TryGetValue(relativePath, out TransferettoSyncEntry? destinationEntry);
            if (context.HasAncestor(relativePath, blockedDirectories)) {
                blockedDirectories.Add(relativePath);
                plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceDirectory, destinationEntry, resolvedOptions, "Destination ancestor cannot be created or replaced."));
            } else if (destinationEntry is null && resolvedOptions.CreateDestinationDirectories) {
                plan.Add(CreatePlanItem(TransferettoSyncAction.CreateDirectory, sourceDirectory, null, resolvedOptions, "Destination directory is missing."));
            } else if (destinationEntry is { IsDirectory: false } && resolvedOptions.CreateDestinationDirectories && resolvedOptions.OverwriteExisting) {
                plan.Add(CreatePlanItem(GetDeleteFileAction(resolvedOptions), null, destinationEntry, resolvedOptions, "Destination file conflicts with source directory."));
                plan.Add(CreatePlanItem(TransferettoSyncAction.CreateDirectory, sourceDirectory, null, resolvedOptions, "Destination directory replaces conflicting file."));
            } else {
                if (destinationEntry is null || !destinationEntry.IsDirectory) { blockedDirectories.Add(relativePath); }
                plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceDirectory, destinationEntry, resolvedOptions, "Directory already exists or directory creation is disabled."));
            }
        }

        foreach (string relativePath in includedSourceFiles.OrderBy(static path => path, StringComparer.Ordinal)) {
            cancellationToken.ThrowIfCancellationRequested();
            TransferettoSyncEntry sourceFile = source[relativePath];
            destination.TryGetValue(relativePath, out TransferettoSyncEntry? destinationEntry);
            TransferettoSyncAction transferAction = resolvedOptions.Direction == TransferettoSyncDirection.Upload
                ? TransferettoSyncAction.UploadFile
                : TransferettoSyncAction.DownloadFile;

            if (context.HasAncestor(relativePath, blockedDirectories) || HasUnreplaceableParent(relativePath, source, destination, resolvedOptions)) {
                plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, destinationEntry, resolvedOptions, "Destination ancestor cannot be created or replaced."));
                continue;
            }

            if (destinationEntry is null) {
                string parentPath = GetParentRelativePath(relativePath);
                if (!resolvedOptions.CreateDestinationDirectories
                    && !string.IsNullOrEmpty(parentPath)
                    && !destination.TryGetValue(parentPath, out TransferettoSyncEntry? parentEntry)) {
                    plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, null, resolvedOptions, "Destination parent directory is missing and directory creation is disabled."));
                    continue;
                }

                if (!string.IsNullOrEmpty(parentPath)
                    && destination.TryGetValue(parentPath, out parentEntry)
                    && !parentEntry.IsDirectory
                    && (!resolvedOptions.CreateDestinationDirectories
                        || !source.TryGetValue(parentPath, out TransferettoSyncEntry? sourceParentEntry)
                        || !sourceParentEntry.IsDirectory)) {
                    plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, parentEntry, resolvedOptions, "Destination parent path is a file."));
                    continue;
                }

                plan.Add(CreatePlanItem(transferAction, sourceFile, null, resolvedOptions, "Destination file is missing."));
                continue;
            }

            if (destinationEntry.IsDirectory) {
                if (resolvedOptions.Mode == TransferettoSyncMode.Mirror && resolvedOptions.OverwriteExisting) {
                    directoryReplacementTransfers.Add((sourceFile, destinationEntry, transferAction));
                } else {
                    plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, destinationEntry, resolvedOptions, "Destination path is a directory."));
                }

                continue;
            }

            if (!ShouldTransfer(sourceFile, destinationEntry, resolvedOptions)) {
                plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, destinationEntry, resolvedOptions, "Destination file is current."));
                continue;
            }

            plan.Add(resolvedOptions.OverwriteExisting
                ? CreatePlanItem(transferAction, sourceFile, destinationEntry, resolvedOptions, "Destination file differs.")
                : CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, destinationEntry, resolvedOptions, "Destination file differs but overwrite is disabled."));
        }

        if (resolvedOptions.Mode == TransferettoSyncMode.Mirror) {
            HashSet<string> replacementDirectoryPaths = new(
                directoryReplacementTransfers.Select(item => NormalizeRelativePath(item.Destination.RelativePath)),
            context.Comparer);
            AddMirrorDeletes(plan, source, destination, context, replacementDirectoryPaths, true);
            AddDirectoryReplacementTransfers(plan, directoryReplacementTransfers, resolvedOptions, context.Comparer);
            AddMirrorDeletes(plan, source, destination, context, replacementDirectoryPaths, false);
        }

        return plan;
    }

    internal static string NormalizeRelativePath(string path) {
        string normalized = path.Trim('/');
        while (normalized.Contains("//")) {
            normalized = normalized.Replace("//", "/");
        }

        return normalized;
    }

    private static bool ShouldTransfer(TransferettoSyncEntry source, TransferettoSyncEntry destination, TransferettoSyncOptions options) {
        return options.Comparison switch {
            TransferettoSyncComparison.Always => true,
            TransferettoSyncComparison.Size => SizeDiffers(source, destination),
            TransferettoSyncComparison.LastWriteTime => TimestampDiffers(source, destination, options.TimestampTolerance),
            _ => SizeDiffers(source, destination) || TimestampDiffers(source, destination, options.TimestampTolerance)
        };
    }

    private static bool SizeDiffers(TransferettoSyncEntry source, TransferettoSyncEntry destination) {
        return source.Length.HasValue && destination.Length.HasValue && source.Length.Value != destination.Length.Value;
    }

    private static bool TimestampDiffers(TransferettoSyncEntry source, TransferettoSyncEntry destination, TimeSpan tolerance) {
        if (!source.LastWriteTimeUtc.HasValue || !destination.LastWriteTimeUtc.HasValue) {
            return false;
        }

        return (source.LastWriteTimeUtc.Value - destination.LastWriteTimeUtc.Value).Duration() > tolerance;
    }

    private static void AddMirrorDeletes(
        ICollection<TransferettoSyncPlanItem> plan,
        IReadOnlyDictionary<string, TransferettoSyncEntry> source,
        IReadOnlyDictionary<string, TransferettoSyncEntry> destination,
        PlannerContext context,
        ISet<string> replacementDirectoryPaths,
        bool replacementOnly) {
        TransferettoSyncOptions options = context.Options;
        TransferettoSyncAction deleteFile = options.Direction == TransferettoSyncDirection.Upload
            ? TransferettoSyncAction.DeleteRemoteFile
            : TransferettoSyncAction.DeleteLocalFile;
        TransferettoSyncAction deleteDirectory = GetDeleteDirectoryAction(options);
        TransferettoSyncEntry[] extraFiles = destination
            .Where(pair => !pair.Value.IsDirectory && !source.ContainsKey(pair.Key) && context.IsIncluded(pair.Value.RelativePath))
            .Where(pair => (replacementDirectoryPaths.Contains(pair.Key) || context.HasAncestor(pair.Key, replacementDirectoryPaths)) == replacementOnly)
            .Select(static pair => pair.Value)
            .OrderByDescending(entry => entry.RelativePath.Count(static character => character == '/'))
            .ThenByDescending(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();
        HashSet<string> extraFilePaths = new(extraFiles.Select(entry => NormalizeRelativePath(entry.RelativePath)), context.Comparer);
        HashSet<string> extraFileAncestors = context.GetAncestors(extraFilePaths);
        HashSet<string> blocked = new(context.Comparer);
        List<TransferettoSyncEntry> extraDirectories = new();
        // Process children before parents once. A retained child blocks every ancestor, including
        // implicit directories missing from the manifest; no recursive rescans of all descendants.
        foreach (KeyValuePair<string, TransferettoSyncEntry> pair in destination
                     .OrderByDescending(pair => pair.Key.Count(static character => character == '/'))
                     .ThenByDescending(pair => pair.Key, StringComparer.Ordinal)) {
            context.CancellationToken.ThrowIfCancellationRequested();
            bool deletable = pair.Value.IsDirectory
                ? (!source.ContainsKey(pair.Key) || replacementDirectoryPaths.Contains(pair.Key))
                    && !blocked.Contains(pair.Key)
                    && (replacementDirectoryPaths.Contains(pair.Key) || context.IsIncluded(pair.Key) || extraFileAncestors.Contains(pair.Key))
                    && (replacementDirectoryPaths.Contains(pair.Key) || context.HasAncestor(pair.Key, replacementDirectoryPaths)) == replacementOnly
                : extraFilePaths.Contains(pair.Key);
            if (!deletable) {
                context.AddAncestors(pair.Key, blocked);
            } else if (pair.Value.IsDirectory) {
                extraDirectories.Add(pair.Value);
            }
        }

        foreach (TransferettoSyncEntry file in extraFiles) {
            plan.Add(CreatePlanItem(deleteFile, null, file, options, "Destination file is not present in source."));
        }

        foreach (TransferettoSyncEntry directory in extraDirectories) {
            plan.Add(CreatePlanItem(deleteDirectory, null, directory, options, "Destination directory is not present in source."));
        }
    }

    private static void AddDirectoryReplacementTransfers(
        ICollection<TransferettoSyncPlanItem> plan,
        IEnumerable<(TransferettoSyncEntry Source, TransferettoSyncEntry Destination, TransferettoSyncAction TransferAction)> directoryReplacementTransfers,
        TransferettoSyncOptions options,
        StringComparer comparer) {
        TransferettoSyncAction deleteDirectoryAction = GetDeleteDirectoryAction(options);
        HashSet<string> deletedDirectoryPaths = new(
            plan
                .Where(item => item.Action == deleteDirectoryAction)
                .Select(item => NormalizeRelativePath(item.RelativePath)),
            comparer);

        foreach ((TransferettoSyncEntry source, TransferettoSyncEntry destination, TransferettoSyncAction transferAction) in directoryReplacementTransfers) {
            string relativePath = NormalizeRelativePath(destination.RelativePath);
            plan.Add(deletedDirectoryPaths.Contains(relativePath)
                ? CreatePlanItem(transferAction, source, destination, options, "Destination directory is replaced by source file.")
                : CreatePlanItem(TransferettoSyncAction.Skip, source, destination, options, "Destination directory contains content that cannot be replaced."));
        }
    }

    private static bool HasUnreplaceableParent(
        string relativePath,
        IReadOnlyDictionary<string, TransferettoSyncEntry> source,
        IReadOnlyDictionary<string, TransferettoSyncEntry> destination,
        TransferettoSyncOptions options) {
        for (string parent = GetParentRelativePath(relativePath); parent.Length > 0; parent = GetParentRelativePath(parent)) {
            if (destination.TryGetValue(parent, out TransferettoSyncEntry? entry) && !entry.IsDirectory
                && (!options.CreateDestinationDirectories || !options.OverwriteExisting
                    || !source.TryGetValue(parent, out TransferettoSyncEntry? sourceEntry) || !sourceEntry.IsDirectory)) {
                return true;
            }
        }
        return false;
    }

    private static string GetParentRelativePath(string relativePath) {
        string normalized = NormalizeRelativePath(relativePath);
        int separatorIndex = normalized.LastIndexOf('/');
        return separatorIndex <= 0
            ? string.Empty
            : normalized.Substring(0, separatorIndex);
    }

    private static TransferettoSyncAction GetDeleteFileAction(TransferettoSyncOptions options) {
        return options.Direction == TransferettoSyncDirection.Upload
            ? TransferettoSyncAction.DeleteRemoteFile
            : TransferettoSyncAction.DeleteLocalFile;
    }

    private static TransferettoSyncAction GetDeleteDirectoryAction(TransferettoSyncOptions options) {
        return options.Direction == TransferettoSyncDirection.Upload
            ? TransferettoSyncAction.DeleteRemoteDirectory
            : TransferettoSyncAction.DeleteLocalDirectory;
    }

    private static TransferettoSyncPlanItem CreatePlanItem(
        TransferettoSyncAction action,
        TransferettoSyncEntry? source,
        TransferettoSyncEntry? destination,
        TransferettoSyncOptions options,
        string message) {
        TransferettoSyncEntry? item = source ?? destination;
        return new TransferettoSyncPlanItem {
            Action = action,
            Direction = options.Direction,
            RelativePath = item?.RelativePath ?? string.Empty,
            LocalPath = source?.LocalPath ?? destination?.LocalPath,
            RemotePath = source?.RemotePath ?? destination?.RemotePath,
            Source = source,
            Destination = destination,
            Message = message
        };
    }
}
