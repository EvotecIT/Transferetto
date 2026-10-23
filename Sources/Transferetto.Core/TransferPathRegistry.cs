using System;
using System.Collections.Generic;
using System.IO;

namespace Transferetto.Core;

/// <summary>Rejects distinct source names that map to one local file or directory during a transfer.</summary>
public sealed class TransferPathRegistry {
    private readonly string _root;
    private readonly Dictionary<string, string> _names = new(StringComparer.Ordinal);
    private readonly HashSet<string> _items = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StringComparison> _comparisons = new(StringComparer.Ordinal);

    /// <summary>Initializes an operation-wide registry rooted at the destination directory.</summary>
    public TransferPathRegistry(string root) { _root = Path.GetFullPath(root); }

    /// <summary>Validates a slash-separated name and its ancestors, then returns the resolved local path.</summary>
    /// <remarks>Repeated identical ancestors are allowed; distinct spellings of one destination are rejected.
    /// This does not reserve a file or protect against concurrent filesystem mutation.</remarks>
    public string Add(string relativePath) {
        string result = TransferFileSystem.ResolveRelativePath(_root, relativePath);
        if (!_items.Add(relativePath)) { throw new InvalidDataException($"Duplicate source entry: {relativePath}"); }
        string relative = string.Empty;
        foreach (string segment in relativePath.Split('/')) {
            relative = relative.Length == 0 ? segment : relative + "/" + segment;
            string full = TransferFileSystem.ResolveRelativePath(_root, relative);
            string identity = TransferFileSystem.GetPathIdentity(full, _comparisons);
            if (_names.TryGetValue(identity, out string? previous) && previous != relative) {
                throw new InvalidDataException($"Distinct source names map to the same destination: {previous} and {relative}");
            }
            _names[identity] = relative;
        }
        return result;
    }
}
