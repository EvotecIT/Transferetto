using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Renci.SshNet.Common;

namespace Transferetto;

public static partial class TransferettoClient {
    private static string NormalizeFingerprint(string? fingerprint) {
        string normalized = (fingerprint ?? string.Empty).Trim();
        if (normalized.Length == 0) { return string.Empty; }
        bool sha256 = false;
        bool md5 = false;
        if (normalized.StartsWith("SHA256:", StringComparison.OrdinalIgnoreCase)) {
            sha256 = true;
            normalized = normalized.Substring("SHA256:".Length);
        } else if (normalized.StartsWith("SHA256", StringComparison.OrdinalIgnoreCase)) {
            sha256 = true;
            normalized = normalized.Substring("SHA256".Length);
        } else if (normalized.StartsWith("MD5:", StringComparison.OrdinalIgnoreCase)) {
            md5 = true;
            normalized = normalized.Substring("MD5:".Length);
        } else if (normalized.StartsWith("MD5", StringComparison.OrdinalIgnoreCase)) {
            md5 = true;
            normalized = normalized.Substring("MD5".Length);
        }
        string hex = normalized.Replace(":", string.Empty).Replace("-", string.Empty);
        if (!sha256 && TransferettoTrustStore.IsHexFingerprint(hex, 32)) { return "MD5:" + hex.ToLowerInvariant(); }
        if (md5) { throw new FormatException("An MD5 host fingerprint must contain 16 hexadecimal bytes."); }
        string unpadded = normalized.TrimEnd('=');
        if (unpadded.Any(char.IsWhiteSpace)) { throw new FormatException("An SSH SHA256 fingerprint cannot contain whitespace."); }
        byte[] bytes = Convert.FromBase64String(unpadded + new string('=', (4 - unpadded.Length % 4) % 4));
        if (bytes.Length != 32) { throw new FormatException("An SSH SHA256 fingerprint must contain 32 bytes."); }
        return "SHA256:" + Convert.ToBase64String(bytes).TrimEnd('=');
    }

    private static TransferettoSshHostKeyInfo EvaluateHostKeyTrust(TransferettoSshConnectionOptions options, HostKeyEventArgs args) {
        TransferettoSshHostKeyInfo hostKeyInfo = CreateHostKeyInfo(args);

        if (options.AcceptAnyHostKey) {
            hostKeyInfo.CanTrust = true;
            hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.AcceptAny;
            return hostKeyInfo;
        }

        string[] expectedFingerprints = options.ExpectedHostKeyFingerprints?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeFingerprint)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? Array.Empty<string>();

        if (expectedFingerprints.Length > 0) {
            hostKeyInfo.CanTrust = FingerprintMatches(expectedFingerprints, hostKeyInfo);
            hostKeyInfo.TrustSource = hostKeyInfo.CanTrust
                ? TransferettoSshHostKeyTrustSource.ExpectedFingerprint
                : TransferettoSshHostKeyTrustSource.None;
            return hostKeyInfo;
        }

        switch (options.HostKeyPolicy) {
            case TransferettoSshHostKeyPolicy.Loose:
                hostKeyInfo.CanTrust = true;
                hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.Loose;
                return hostKeyInfo;
            case TransferettoSshHostKeyPolicy.KnownHosts:
                return EvaluateKnownHostsTrust(options, hostKeyInfo, false);
            case TransferettoSshHostKeyPolicy.TrustOnFirstUse:
            default:
                return EvaluateKnownHostsTrust(options, hostKeyInfo, true);
        }
    }

    private static TransferettoSshHostKeyInfo EvaluateKnownHostsTrust(
        TransferettoSshConnectionOptions options,
        TransferettoSshHostKeyInfo hostKeyInfo,
        bool trustOnFirstUse) {
        string knownHostsPath = ResolveKnownHostsPath(options);
        hostKeyInfo.KnownHostsPath = knownHostsPath;
        using FileStream transaction = TransferettoTrustStore.Acquire(knownHostsPath);
        List<TransferettoSshKnownHostEntry> entries = LoadKnownHosts(knownHostsPath);
        TransferettoSshKnownHostEntry[] matchingEntries = entries
            .Where(entry => string.Equals(entry.Host, options.Server, StringComparison.OrdinalIgnoreCase) && entry.Port == (options.Port ?? 22))
            .ToArray();

        if (matchingEntries.Length == 0) {
            if (!trustOnFirstUse) {
                hostKeyInfo.CanTrust = false;
                hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.None;
                return hostKeyInfo;
            }

            entries.Add(CreateKnownHostEntry(options, hostKeyInfo));
            SaveKnownHosts(knownHostsPath, entries);
            hostKeyInfo.CanTrust = true;
            hostKeyInfo.TrustSource = TransferettoSshHostKeyTrustSource.TrustOnFirstUse;
            hostKeyInfo.WasPersisted = true;
            return hostKeyInfo;
        }

        TransferettoSshKnownHostEntry? trustedEntry = matchingEntries.FirstOrDefault(entry => KnownHostMatches(entry, hostKeyInfo));
        bool isTrusted = trustedEntry is not null;
        if (trustedEntry is not null) {
            trustedEntry.LastSeenUtc = DateTime.UtcNow.ToString("O");
            SaveKnownHosts(knownHostsPath, entries);
        }

        hostKeyInfo.CanTrust = isTrusted;
        hostKeyInfo.TrustSource = isTrusted
            ? TransferettoSshHostKeyTrustSource.KnownHosts
            : TransferettoSshHostKeyTrustSource.None;
        return hostKeyInfo;
    }

    private static TransferettoSshHostKeyInfo CreateHostKeyInfo(HostKeyEventArgs args) {
        return new TransferettoSshHostKeyInfo {
            HostKeyName = args.HostKeyName,
            KeyLength = args.KeyLength,
            FingerPrintMD5 = args.FingerPrintMD5,
            FingerPrintSHA256 = args.FingerPrintSHA256
        };
    }

    private static bool FingerprintMatches(IEnumerable<string> expectedFingerprints, TransferettoSshHostKeyInfo hostKeyInfo) {
        string normalizedMd5 = NormalizeFingerprint(hostKeyInfo.FingerPrintMD5);
        string normalizedSha256 = NormalizeFingerprint(hostKeyInfo.FingerPrintSHA256);

        return expectedFingerprints.Any(expected => NormalizeFingerprint(expected) is string value
            && value.Length > 0 && (value == normalizedMd5 || value == normalizedSha256));
    }

    private static bool KnownHostMatches(TransferettoSshKnownHostEntry entry, TransferettoSshHostKeyInfo hostKeyInfo) {
        if (!string.Equals(entry.HostKeyName, hostKeyInfo.HostKeyName, StringComparison.Ordinal)) {
            return false;
        }

        return FingerprintMatches(
            new[] {
                NormalizeFingerprint(entry.FingerPrintMD5),
                NormalizeFingerprint(entry.FingerPrintSHA256)
            }.Where(static value => !string.IsNullOrWhiteSpace(value)),
            hostKeyInfo);
    }

    private static string ResolveKnownHostsPath(TransferettoSshConnectionOptions options) {
        if (!string.IsNullOrWhiteSpace(options.KnownHostsPath)) {
            return options.KnownHostsPath!;
        }

        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root)) {
            root = AppDomain.CurrentDomain.BaseDirectory;
        }

        return Path.Combine(root, "Transferetto", "ssh-known-hosts.tsv");
    }

    private static List<TransferettoSshKnownHostEntry> LoadKnownHosts(string path) {
        if (!File.Exists(path)) {
            return new List<TransferettoSshKnownHostEntry>();
        }

        List<TransferettoSshKnownHostEntry> entries = new();
        foreach (string rawLine in TransferettoTrustStore.Read(path)) {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) {
                continue;
            }

            string[] parts = line.Split('\t');
            TransferettoTrustStore.ValidateRecord(parts, path);
            int port = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            if (!int.TryParse(parts[5], out int keyLength) || keyLength < 1 || string.IsNullOrWhiteSpace(parts[2])
                || string.IsNullOrWhiteSpace(parts[3]) && string.IsNullOrWhiteSpace(parts[4])) {
                throw new InvalidDataException($"The known-host store contains an invalid key: {path}");
            }
            try { NormalizeFingerprint(parts[3]); NormalizeFingerprint(parts[4]); }
            catch (FormatException exception) { throw new InvalidDataException($"The known-host store contains an invalid fingerprint: {path}", exception); }

            entries.Add(new TransferettoSshKnownHostEntry {
                Host = parts[0],
                Port = port,
                HostKeyName = parts[2],
                FingerPrintMD5 = parts[3],
                FingerPrintSHA256 = parts[4],
                KeyLength = keyLength,
                FirstSeenUtc = parts[6],
                LastSeenUtc = parts[7]
            });
        }

        return entries;
    }

    private static void SaveKnownHosts(string path, IEnumerable<TransferettoSshKnownHostEntry> entries) {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        string[] lines = entries.Select(SerializeKnownHostEntry).ToArray();
        TransferettoTrustStore.Save(path, lines);
    }

    private static string SerializeKnownHostEntry(TransferettoSshKnownHostEntry entry) {
        return string.Join("\t", new[] {
            SanitizeKnownHostValue(entry.Host),
            entry.Port.ToString(),
            SanitizeKnownHostValue(entry.HostKeyName),
            SanitizeKnownHostValue(entry.FingerPrintMD5),
            SanitizeKnownHostValue(entry.FingerPrintSHA256),
            entry.KeyLength.ToString(),
            SanitizeKnownHostValue(entry.FirstSeenUtc),
            SanitizeKnownHostValue(entry.LastSeenUtc)
        });
    }

    private static string SanitizeKnownHostValue(string? value) {
        return TransferettoTrustStore.SerializeValue(value);
    }

    private static TransferettoSshKnownHostEntry CreateKnownHostEntry(TransferettoSshConnectionOptions options, TransferettoSshHostKeyInfo hostKeyInfo) {
        string now = DateTime.UtcNow.ToString("O");
        return new TransferettoSshKnownHostEntry {
            Host = options.Server,
            Port = options.Port ?? 22,
            HostKeyName = hostKeyInfo.HostKeyName,
            FingerPrintMD5 = hostKeyInfo.FingerPrintMD5,
            FingerPrintSHA256 = hostKeyInfo.FingerPrintSHA256,
            KeyLength = hostKeyInfo.KeyLength,
            FirstSeenUtc = now,
            LastSeenUtc = now
        };
    }

}
