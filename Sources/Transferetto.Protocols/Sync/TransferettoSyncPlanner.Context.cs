using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Transferetto;

public static partial class TransferettoSyncPlanner {
    private sealed class PlannerContext {
        private readonly Regex[] _include;
        private readonly Regex[] _exclude;
        private readonly Dictionary<string, bool> _inclusion;

        internal PlannerContext(TransferettoSyncOptions options, CancellationToken cancellationToken) {
            Options = options;
            CancellationToken = cancellationToken;
            bool ignoreCase = options.PathComparison == TransferettoSyncPathComparison.OrdinalIgnoreCase
                || (options.PathComparison == TransferettoSyncPathComparison.Automatic
                    && options.Direction == TransferettoSyncDirection.Download
                    && RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            Comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            _inclusion = new Dictionary<string, bool>(Comparer);
            _include = CreatePatterns(options.IncludePatterns);
            _exclude = CreatePatterns(options.ExcludePatterns);
        }

        internal TransferettoSyncOptions Options { get; }
        internal CancellationToken CancellationToken { get; }
        internal StringComparer Comparer { get; }

        internal Dictionary<string, TransferettoSyncEntry> BuildManifest(IEnumerable<TransferettoSyncEntry> entries) {
            Dictionary<string, TransferettoSyncEntry> manifest = new(Comparer);
            foreach (TransferettoSyncEntry entry in entries) {
                CancellationToken.ThrowIfCancellationRequested();
                string path = NormalizeRelativePath(entry.RelativePath);
                if (path.Length == 0) { continue; }
                if (manifest.ContainsKey(path)) {
                    throw new ArgumentException($"Multiple manifest entries map to the same destination path: {path}", nameof(entries));
                }
                manifest.Add(path, entry);
            }
            return manifest;
        }

        internal bool IsIncluded(string relativePath) {
            CancellationToken.ThrowIfCancellationRequested();
            string path = NormalizeRelativePath(relativePath);
            if (_inclusion.TryGetValue(path, out bool included)) { return included; }
            string name = path.Substring(path.LastIndexOf('/') + 1);
            included = (_include.Length == 0 || _include.Any(pattern => pattern.IsMatch(path) || pattern.IsMatch(name)))
                && !_exclude.Any(pattern => pattern.IsMatch(path) || pattern.IsMatch(name));
            _inclusion.Add(path, included);
            return included;
        }

        internal HashSet<string> GetAncestors(IEnumerable<string> paths) {
            HashSet<string> ancestors = new(Comparer);
            foreach (string path in paths) { AddAncestors(path, ancestors); }
            return ancestors;
        }

        internal void AddAncestors(string path, ISet<string> ancestors) {
            for (string parent = GetParentRelativePath(path); parent.Length > 0; parent = GetParentRelativePath(parent)) {
                CancellationToken.ThrowIfCancellationRequested();
                ancestors.Add(parent);
            }
        }

        internal bool HasAncestor(string path, ISet<string> candidates) {
            for (string parent = GetParentRelativePath(path); parent.Length > 0; parent = GetParentRelativePath(parent)) {
                if (candidates.Contains(parent)) { return true; }
            }
            return false;
        }

        private static Regex[] CreatePatterns(string[]? patterns) => (patterns ?? Array.Empty<string>())
            .Where(static pattern => !string.IsNullOrWhiteSpace(pattern))
            .Select(pattern => new Regex("^" + Regex.Escape(pattern.Trim('/')).Replace("\\*", ".*").Replace("\\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)))
            .ToArray();
    }
}
