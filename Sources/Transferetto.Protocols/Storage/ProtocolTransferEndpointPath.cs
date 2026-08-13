using System;
using System.Linq;
using Transferetto.Core;

namespace Transferetto;

internal static class ProtocolTransferEndpointPath {
    internal static string NormalizeRoot(string? root) {
        if (string.IsNullOrWhiteSpace(root)) {
            return string.Empty;
        }

        string normalized = root!.Replace('\\', '/').Trim();
        bool absolute = normalized.StartsWith("/", StringComparison.Ordinal);
        string[] segments = NormalizeSegments(normalized, nameof(root));
        string result = string.Join("/", segments);
        return absolute ? "/" + result : result;
    }

    internal static string NormalizeRelative(string? path, bool allowEmpty = false) {
        if (string.IsNullOrWhiteSpace(path)) {
            if (allowEmpty) {
                return string.Empty;
            }
            throw new ArgumentException("An endpoint-relative path is required.", nameof(path));
        }

        string normalized = path!.Replace('\\', '/').Trim();
        if (normalized.StartsWith("/", StringComparison.Ordinal)) {
            throw new ArgumentException("The path must be relative to the endpoint root.", nameof(path));
        }
        string result = string.Join("/", NormalizeSegments(normalized, nameof(path)));
        if (result.Length == 0 && !allowEmpty) {
            throw new ArgumentException("An endpoint-relative path is required.", nameof(path));
        }
        return result;
    }

    internal static string Resolve(string root, string? path, bool allowEmpty = false) {
        string relative = NormalizeRelative(path, allowEmpty);
        if (string.IsNullOrEmpty(root)) {
            return relative;
        }
        if (string.IsNullOrEmpty(relative)) {
            return root;
        }
        return root.EndsWith("/", StringComparison.Ordinal)
            ? root + relative
            : root + "/" + relative;
    }

    internal static string CombineRelative(string parent, string child) {
        string normalizedChild = NormalizeRelative(child);
        return string.IsNullOrEmpty(parent)
            ? normalizedChild
            : parent.TrimEnd('/') + "/" + normalizedChild;
    }

    internal static string? GetParent(string path) {
        int separator = path.LastIndexOf('/');
        if (separator < 0) {
            return null;
        }
        if (separator == 0) {
            return "/";
        }
        return path.Substring(0, separator);
    }

    internal static string CreateTemporaryPath(string destinationPath) =>
        destinationPath + ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp";

    internal static long? NormalizeLength(long length) => length >= 0 ? length : null;

    internal static void ValidateUnsupportedMetadata(TransferWriteOptions options, string scheme) {
        if (!string.IsNullOrWhiteSpace(options.ContentType) || options.Metadata.Count > 0) {
            throw new NotSupportedException(
                $"The {scheme} endpoint cannot persist content type or object metadata.");
        }
    }

    private static string[] NormalizeSegments(string path, string parameterName) {
        string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(segment => segment != ".")
            .ToArray();
        if (segments.Any(segment => segment == "..")) {
            throw new ArgumentException("The path cannot escape the endpoint root.", parameterName);
        }
        return segments;
    }
}
