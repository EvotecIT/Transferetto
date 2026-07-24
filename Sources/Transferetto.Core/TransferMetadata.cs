using System;
using System.Collections.Generic;

namespace Transferetto.Core;

/// <summary>
/// Validates portable user metadata shared between transfer providers.
/// </summary>
public static class TransferMetadata {
    /// <summary>
    /// Copies metadata after validating names that are portable across S3 and Azure Blob Storage.
    /// </summary>
    public static Dictionary<string, string> CopyPortable(
        IEnumerable<KeyValuePair<string, string>> metadata,
        string parameterName = "metadata") {
        if (metadata == null) {
            throw new ArgumentNullException(nameof(metadata));
        }

        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> pair in metadata) {
            ValidateName(pair.Key, parameterName);
            if (pair.Value == null) {
                throw new ArgumentException("Metadata values cannot be null.", parameterName);
            }
            result[pair.Key] = pair.Value;
        }
        return result;
    }

    /// <summary>
    /// Validates a provider-neutral metadata name.
    /// </summary>
    public static void ValidateName(string name, string parameterName = "name") {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException("Metadata names cannot be empty.", parameterName);
        }
        if (!(IsAsciiLetter(name[0]) || name[0] == '_')) {
            throw new ArgumentException(
                $"Metadata name '{name}' must start with a letter or underscore.",
                parameterName);
        }
        for (int index = 1; index < name.Length; index++) {
            if (!(IsAsciiLetter(name[index]) || IsAsciiDigit(name[index]) || name[index] == '_')) {
                throw new ArgumentException(
                    $"Metadata name '{name}' may contain only letters, digits, and underscores.",
                    parameterName);
            }
        }
    }

    private static bool IsAsciiLetter(char value) =>
        value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';
}
