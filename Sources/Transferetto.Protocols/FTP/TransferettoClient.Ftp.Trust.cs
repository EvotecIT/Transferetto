using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentFTP;

namespace Transferetto;

public static partial class TransferettoClient {
    private static TransferettoFtpCertificateInfo EvaluateKnownCertificateTrust(
        TransferettoFtpConnectionOptions options,
        FtpClient client,
        TransferettoFtpCertificateInfo certificateInfo,
        bool trustOnFirstUse) {
        string knownCertificatesPath = ResolveKnownCertificatesPath(options);
        certificateInfo.KnownCertificatesPath = knownCertificatesPath;
        string host = ResolveFtpTrustHost(options, client);
        int port = ResolveFtpTrustPort(options, client);
        using FileStream transaction = TransferettoTrustStore.Acquire(knownCertificatesPath);
        List<TransferettoFtpKnownCertificateEntry> entries = LoadKnownCertificates(knownCertificatesPath);
        TransferettoFtpKnownCertificateEntry[] matchingEntries = entries
            .Where(entry => string.Equals(entry.Host, host, StringComparison.OrdinalIgnoreCase) && entry.Port == port)
            .ToArray();

        if (matchingEntries.Length == 0) {
            if (!trustOnFirstUse) {
                certificateInfo.CanTrust = false;
                certificateInfo.TrustSource = TransferettoFtpCertificateTrustSource.None;
                return certificateInfo;
            }

            entries.Add(CreateKnownCertificateEntry(options, client, certificateInfo));
            SaveKnownCertificates(knownCertificatesPath, entries);
            certificateInfo.CanTrust = true;
            certificateInfo.TrustSource = TransferettoFtpCertificateTrustSource.TrustOnFirstUse;
            certificateInfo.WasPersisted = true;
            return certificateInfo;
        }

        TransferettoFtpKnownCertificateEntry? trustedEntry = matchingEntries.FirstOrDefault(entry => KnownCertificateMatches(entry, certificateInfo));
        bool isTrusted = trustedEntry is not null;
        if (trustedEntry is not null) {
            trustedEntry.LastSeenUtc = DateTime.UtcNow.ToString("O");
            SaveKnownCertificates(knownCertificatesPath, entries);
        }

        certificateInfo.CanTrust = isTrusted;
        certificateInfo.TrustSource = isTrusted
            ? TransferettoFtpCertificateTrustSource.KnownCertificates
            : TransferettoFtpCertificateTrustSource.None;
        return certificateInfo;
    }

    private static TransferettoFtpCertificateInfo CreateFtpCertificateInfo(FtpSslValidationEventArgs args) {
        if (args.Certificate is null) {
            return new TransferettoFtpCertificateInfo {
                PolicyErrors = args.PolicyErrors.ToString(),
                CanTrust = false,
                TrustSource = TransferettoFtpCertificateTrustSource.None
            };
        }

        X509Certificate2 certificate = args.Certificate as X509Certificate2 ?? new X509Certificate2(args.Certificate);
        try {
            return new TransferettoFtpCertificateInfo {
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                ThumbprintSHA1 = NormalizeCertificateThumbprint(certificate.Thumbprint),
                ThumbprintSHA256 = GetCertificateHash(certificate, SHA256.Create()),
                NotBefore = certificate.NotBefore,
                NotAfter = certificate.NotAfter,
                PolicyErrors = args.PolicyErrors == SslPolicyErrors.None ? null : args.PolicyErrors.ToString()
            };
        } finally {
            if (!ReferenceEquals(certificate, args.Certificate)) {
                certificate.Dispose();
            }
        }
    }

    private static bool CertificateThumbprintMatches(IEnumerable<string> expectedThumbprints, TransferettoFtpCertificateInfo certificateInfo) {
        HashSet<string> actualThumbprints = new(StringComparer.OrdinalIgnoreCase);
        AddCertificateThumbprint(actualThumbprints, "SHA1", certificateInfo.ThumbprintSHA1);
        AddCertificateThumbprint(actualThumbprints, "SHA256", certificateInfo.ThumbprintSHA256);

        foreach (string expectedThumbprint in expectedThumbprints.Where(static value => !string.IsNullOrWhiteSpace(value))) {
            string normalizedExpected = NormalizeCertificateThumbprint(expectedThumbprint);
            if (actualThumbprints.Contains(normalizedExpected)) {
                return true;
            }
        }

        return false;
    }

    private static bool KnownCertificateMatches(TransferettoFtpKnownCertificateEntry entry, TransferettoFtpCertificateInfo certificateInfo) {
        return CertificateThumbprintMatches(
            new[] {
                entry.ThumbprintSHA1,
                entry.ThumbprintSHA256
            }.Where(static value => !string.IsNullOrWhiteSpace(value))!,
            certificateInfo);
    }

    private static bool HasExpectedCertificateThumbprints(TransferettoFtpConnectionOptions options) {
        return options.ExpectedCertificateThumbprints?.Any(static value => !string.IsNullOrWhiteSpace(value)) == true;
    }

    private static string ResolveKnownCertificatesPath(TransferettoFtpConnectionOptions options) {
        if (!string.IsNullOrWhiteSpace(options.KnownCertificatesPath)) {
            return options.KnownCertificatesPath!;
        }

        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root)) {
            root = AppDomain.CurrentDomain.BaseDirectory;
        }

        return Path.Combine(root, "Transferetto", "ftps-known-certificates.tsv");
    }

    private static int ResolveFtpPort(TransferettoFtpConnectionOptions options) {
        if (options.Port.HasValue && options.Port.Value > 0) {
            return options.Port.Value;
        }

        return options.EncryptionMode?.Contains(FtpEncryptionMode.Implicit) == true ? 990 : 21;
    }

    private static List<TransferettoFtpKnownCertificateEntry> LoadKnownCertificates(string path) {
        if (!File.Exists(path)) {
            return new List<TransferettoFtpKnownCertificateEntry>();
        }

        List<TransferettoFtpKnownCertificateEntry> entries = new();
        foreach (string rawLine in TransferettoTrustStore.Read(path)) {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) {
                continue;
            }

            string[] parts = line.Split('\t');
            TransferettoTrustStore.ValidateRecord(parts, path);
            int port = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            string sha1 = NormalizeCertificateThumbprint(parts[4]).Replace("SHA1:", string.Empty);
            string sha256 = NormalizeCertificateThumbprint(parts[5]).Replace("SHA256:", string.Empty);
            if ((sha1.Length == 0 && sha256.Length == 0)
                || sha1.Length > 0 && !TransferettoTrustStore.IsHexFingerprint(sha1, 40)
                || sha256.Length > 0 && !TransferettoTrustStore.IsHexFingerprint(sha256, 64)) {
                throw new InvalidDataException($"The certificate store contains an invalid fingerprint: {path}");
            }

            entries.Add(new TransferettoFtpKnownCertificateEntry {
                Host = parts[0],
                Port = port,
                Subject = parts[2],
                Issuer = parts[3],
                ThumbprintSHA1 = parts[4],
                ThumbprintSHA256 = parts[5],
                FirstSeenUtc = parts[6],
                LastSeenUtc = parts[7]
            });
        }

        return entries;
    }

    private static void SaveKnownCertificates(string path, IEnumerable<TransferettoFtpKnownCertificateEntry> entries) {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        string[] lines = entries.Select(SerializeKnownCertificateEntry).ToArray();
        TransferettoTrustStore.Save(path, lines);
    }

    private static string SerializeKnownCertificateEntry(TransferettoFtpKnownCertificateEntry entry) {
        return string.Join("\t", new[] {
            SanitizeKnownCertificateValue(entry.Host),
            entry.Port.ToString(),
            SanitizeKnownCertificateValue(entry.Subject),
            SanitizeKnownCertificateValue(entry.Issuer),
            SanitizeKnownCertificateValue(entry.ThumbprintSHA1),
            SanitizeKnownCertificateValue(entry.ThumbprintSHA256),
            SanitizeKnownCertificateValue(entry.FirstSeenUtc),
            SanitizeKnownCertificateValue(entry.LastSeenUtc)
        });
    }

    private static TransferettoFtpKnownCertificateEntry CreateKnownCertificateEntry(
        TransferettoFtpConnectionOptions options,
        FtpClient client,
        TransferettoFtpCertificateInfo certificateInfo) {
        string now = DateTime.UtcNow.ToString("O");
        return new TransferettoFtpKnownCertificateEntry {
            Host = ResolveFtpTrustHost(options, client),
            Port = ResolveFtpTrustPort(options, client),
            Subject = certificateInfo.Subject,
            Issuer = certificateInfo.Issuer,
            ThumbprintSHA1 = certificateInfo.ThumbprintSHA1,
            ThumbprintSHA256 = certificateInfo.ThumbprintSHA256,
            FirstSeenUtc = now,
            LastSeenUtc = now
        };
    }

    private static string ResolveFtpTrustHost(TransferettoFtpConnectionOptions options, FtpClient client) {
        if (!string.IsNullOrWhiteSpace(client.Host)) {
            return client.Host;
        }

        return options.Server ?? string.Empty;
    }

    private static int ResolveFtpTrustPort(TransferettoFtpConnectionOptions options, FtpClient client) {
        if (client.Port > 0) {
            return client.Port;
        }

        return ResolveFtpPort(options);
    }

    private static string SanitizeKnownCertificateValue(string? value) {
        return TransferettoTrustStore.SerializeValue(value);
    }

}
