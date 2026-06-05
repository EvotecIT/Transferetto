using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Transferetto;

/// <summary>
/// Builds protocol-neutral synchronization plans from source and destination manifests.
/// </summary>
public static class TransferettoSyncPlanner {
    /// <summary>
    /// Compares source and destination manifests and returns the ordered actions needed to synchronize the destination.
    /// </summary>
    public static IReadOnlyList<TransferettoSyncPlanItem> Plan(
        IEnumerable<TransferettoSyncEntry> sourceEntries,
        IEnumerable<TransferettoSyncEntry> destinationEntries,
        TransferettoSyncOptions? options = null) {
        TransferettoSyncOptions resolvedOptions = options ?? new TransferettoSyncOptions();
        Dictionary<string, TransferettoSyncEntry> source = sourceEntries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.RelativePath))
            .GroupBy(entry => NormalizeRelativePath(entry.RelativePath), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        Dictionary<string, TransferettoSyncEntry> destination = destinationEntries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.RelativePath))
            .GroupBy(entry => NormalizeRelativePath(entry.RelativePath), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        HashSet<string> includedSourceFiles = new(
            source.Values
                .Where(entry => !entry.IsDirectory && IsIncluded(entry.RelativePath, resolvedOptions))
                .Select(entry => NormalizeRelativePath(entry.RelativePath)),
            StringComparer.Ordinal);

        List<TransferettoSyncPlanItem> plan = new();
        List<(TransferettoSyncEntry Source, TransferettoSyncEntry Destination, TransferettoSyncAction TransferAction)> directoryReplacementTransfers = new();
        foreach (TransferettoSyncEntry sourceDirectory in source.Values
                     .Where(entry => entry.IsDirectory && ShouldIncludeDirectory(entry.RelativePath, includedSourceFiles, resolvedOptions))
                     .OrderBy(entry => entry.RelativePath.Count(static character => character == '/'))
                     .ThenBy(entry => entry.RelativePath, StringComparer.Ordinal)) {
            string relativePath = NormalizeRelativePath(sourceDirectory.RelativePath);
            destination.TryGetValue(relativePath, out TransferettoSyncEntry? destinationEntry);
            if (destinationEntry is null && resolvedOptions.CreateDestinationDirectories) {
                plan.Add(CreatePlanItem(TransferettoSyncAction.CreateDirectory, sourceDirectory, null, resolvedOptions, "Destination directory is missing."));
            } else if (destinationEntry is { IsDirectory: false } && resolvedOptions.CreateDestinationDirectories) {
                plan.Add(CreatePlanItem(GetDeleteFileAction(resolvedOptions), null, destinationEntry, resolvedOptions, "Destination file conflicts with source directory."));
                plan.Add(CreatePlanItem(TransferettoSyncAction.CreateDirectory, sourceDirectory, null, resolvedOptions, "Destination directory replaces conflicting file."));
            } else {
                plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceDirectory, destinationEntry, resolvedOptions, "Directory already exists or directory creation is disabled."));
            }
        }

        foreach (string relativePath in includedSourceFiles.OrderBy(static path => path, StringComparer.Ordinal)) {
            TransferettoSyncEntry sourceFile = source[relativePath];
            destination.TryGetValue(relativePath, out TransferettoSyncEntry? destinationEntry);
            TransferettoSyncAction transferAction = resolvedOptions.Direction == TransferettoSyncDirection.Upload
                ? TransferettoSyncAction.UploadFile
                : TransferettoSyncAction.DownloadFile;

            if (destinationEntry is null) {
                string parentPath = GetParentRelativePath(relativePath);
                if (!resolvedOptions.CreateDestinationDirectories
                    && !string.IsNullOrWhiteSpace(parentPath)
                    && !destination.TryGetValue(parentPath, out TransferettoSyncEntry? parentEntry)) {
                    plan.Add(CreatePlanItem(TransferettoSyncAction.Skip, sourceFile, null, resolvedOptions, "Destination parent directory is missing and directory creation is disabled."));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(parentPath)
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
                StringComparer.Ordinal);
            AddMirrorDeletes(plan, source, destination, resolvedOptions, replacementDirectoryPaths);
            AddDirectoryReplacementTransfers(plan, directoryReplacementTransfers, resolvedOptions);
        }

        return plan;
    }

    internal static string NormalizeRelativePath(string path) {
        string normalized = path.Replace('\\', '/').Trim().Trim('/');
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

    private static bool ShouldIncludeDirectory(string relativePath, HashSet<string> includedSourceFiles, TransferettoSyncOptions options) {
        string normalized = NormalizeRelativePath(relativePath);
        return IsIncluded(normalized, options)
            || includedSourceFiles.Any(file => file.StartsWith(normalized + "/", StringComparison.Ordinal));
    }

    private static bool IsIncluded(string relativePath, TransferettoSyncOptions options) {
        string normalized = NormalizeRelativePath(relativePath);
        string name = normalized.Split('/').LastOrDefault() ?? normalized;
        string[] includePatterns = options.IncludePatterns?.Where(static pattern => !string.IsNullOrWhiteSpace(pattern)).ToArray() ?? Array.Empty<string>();
        string[] excludePatterns = options.ExcludePatterns?.Where(static pattern => !string.IsNullOrWhiteSpace(pattern)).ToArray() ?? Array.Empty<string>();

        bool included = includePatterns.Length == 0 || includePatterns.Any(pattern => WildcardMatches(normalized, pattern) || WildcardMatches(name, pattern));
        bool excluded = excludePatterns.Any(pattern => WildcardMatches(normalized, pattern) || WildcardMatches(name, pattern));
        return included && !excluded;
    }

    private static bool WildcardMatches(string value, string pattern) {
        string regexPattern = "^" + Regex.Escape(pattern.Replace('\\', '/').Trim())
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static void AddMirrorDeletes(
        ICollection<TransferettoSyncPlanItem> plan,
        IReadOnlyDictionary<string, TransferettoSyncEntry> source,
        IEnumerable<KeyValuePair<string, TransferettoSyncEntry>> destination,
        TransferettoSyncOptions options,
        ISet<string> replacementDirectoryPaths) {
        TransferettoSyncAction deleteFile = options.Direction == TransferettoSyncDirection.Upload
            ? TransferettoSyncAction.DeleteRemoteFile
            : TransferettoSyncAction.DeleteLocalFile;
        TransferettoSyncAction deleteDirectory = GetDeleteDirectoryAction(options);
        KeyValuePair<string, TransferettoSyncEntry>[] destinationEntries = destination.ToArray();
        TransferettoSyncEntry[] extraFiles = destination
            .Where(pair => !pair.Value.IsDirectory && !source.ContainsKey(pair.Key) && IsIncluded(pair.Value.RelativePath, options))
            .Select(static pair => pair.Value)
            .OrderByDescending(entry => entry.RelativePath.Count(static character => character == '/'))
            .ThenByDescending(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();
        HashSet<string> extraFilePaths = new(extraFiles.Select(entry => NormalizeRelativePath(entry.RelativePath)), StringComparer.Ordinal);
        TransferettoSyncEntry[] extraDirectories = destinationEntries
            .Where(pair => pair.Value.IsDirectory
                && (!source.ContainsKey(pair.Key) || replacementDirectoryPaths.Contains(pair.Key))
                && IsIncluded(pair.Value.RelativePath, options)
                && CanDeleteDestinationDirectory(pair.Key, source, destinationEntries, extraFilePaths, options))
            .Select(static pair => pair.Value)
            .OrderByDescending(entry => entry.RelativePath.Count(static character => character == '/'))
            .ThenByDescending(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();

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
        TransferettoSyncOptions options) {
        TransferettoSyncAction deleteDirectoryAction = GetDeleteDirectoryAction(options);
        HashSet<string> deletedDirectoryPaths = new(
            plan
                .Where(item => item.Action == deleteDirectoryAction)
                .Select(item => NormalizeRelativePath(item.RelativePath)),
            StringComparer.Ordinal);

        foreach ((TransferettoSyncEntry source, TransferettoSyncEntry destination, TransferettoSyncAction transferAction) in directoryReplacementTransfers) {
            string relativePath = NormalizeRelativePath(destination.RelativePath);
            plan.Add(deletedDirectoryPaths.Contains(relativePath)
                ? CreatePlanItem(transferAction, source, destination, options, "Destination directory is replaced by source file.")
                : CreatePlanItem(TransferettoSyncAction.Skip, source, destination, options, "Destination directory contains content that cannot be replaced."));
        }
    }

    private static bool CanDeleteDestinationDirectory(
        string relativePath,
        IReadOnlyDictionary<string, TransferettoSyncEntry> source,
        IEnumerable<KeyValuePair<string, TransferettoSyncEntry>> destination,
        ISet<string> extraFilePaths,
        TransferettoSyncOptions options) {
        string childPrefix = NormalizeRelativePath(relativePath) + "/";
        foreach (KeyValuePair<string, TransferettoSyncEntry> child in destination.Where(pair => pair.Key.StartsWith(childPrefix, StringComparison.Ordinal))) {
            if (source.ContainsKey(child.Key)) {
                return false;
            }

            if (!IsIncluded(child.Value.RelativePath, options)) {
                return false;
            }

            if (!child.Value.IsDirectory && !extraFilePaths.Contains(child.Key)) {
                return false;
            }
        }

        return true;
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
