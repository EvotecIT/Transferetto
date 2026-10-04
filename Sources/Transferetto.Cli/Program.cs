using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto.Cli;

internal static class Program {
    private static async Task<int> Main(string[] args) {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h") {
            PrintUsage();
            return 0;
        }
        try {
            return args[0] switch {
                "copy" => await CopyAsync(args).ConfigureAwait(false),
                "resume" => await ResumeAsync(args).ConfigureAwait(false),
                "list" => await ListAsync(args).ConfigureAwait(false),
                "inspect" => await InspectAsync(args).ConfigureAwait(false),
                "sync" => await SyncAsync(args).ConfigureAwait(false),
                _ => UsageError("Unknown command.")
            };
        } catch (ArgumentException) {
            Console.Error.WriteLine("Invalid command or endpoint arguments. Run 'transferetto --help' for usage.");
            return 2;
        } catch (FileNotFoundException) {
            Console.Error.WriteLine("The requested source item was not found.");
            return 3;
        } catch (Exception exception) {
            // Provider exceptions may contain signed URLs, credentials, paths, or headers.
            Console.Error.WriteLine($"Transfer failed ({exception.GetType().Name}).");
            return 4;
        }
    }

    private static async Task<int> CopyAsync(string[] args) {
        if (args.Length < 5) { return UsageError("Copy needs two endpoint URIs and two item paths."); }
        HashSet<string> flags = ParseFlags(args, 5, "--overwrite", "--skip-existing", "--verify", "--server-side");
        if (flags.Contains("--overwrite") && flags.Contains("--skip-existing")) {
            return UsageError("Choose only one destination collision mode.");
        }
        using CliEndpoint source = CliEndpoint.Open(args[1]);
        using CliEndpoint destination = CliEndpoint.Open(args[3]);
        TransferCopyOptions options = new() {
            WriteOptions = new TransferWriteOptions { Mode = flags.Contains("--overwrite")
                ? TransferWriteMode.Overwrite : flags.Contains("--skip-existing")
                    ? TransferWriteMode.SkipIfExists : TransferWriteMode.FailIfExists },
            VerifyDestination = flags.Contains("--verify"),
            PreferServerSideCopy = flags.Contains("--server-side")
        };
        TransferReceipt result = await TransferEngine.CopyAsync(source.Endpoint, args[2], destination.Endpoint, args[4], options)
            .ConfigureAwait(false);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{result.Outcome}: {result.BytesTransferred} bytes; SHA-256={result.Sha256 ?? "unavailable"}; provider-side={result.ServerSideCopy}; verified={result.DestinationVerified}"));
        return 0;
    }

    private static async Task<int> ResumeAsync(string[] args) {
        if (args.Length < 6) { return UsageError("Resume needs two endpoint URIs, two item paths, and a checkpoint path."); }
        HashSet<string> flags = ParseFlags(args, 6, "--overwrite", "--skip-existing", "--verify");
        if (flags.Contains("--overwrite") && flags.Contains("--skip-existing")) {
            return UsageError("Choose only one destination collision mode.");
        }
        using CliEndpoint source = CliEndpoint.Open(args[1]);
        using CliEndpoint destination = CliEndpoint.Open(args[3]);
        TransferResumeOptions options = new() {
            CheckpointPath = args[5],
            CopyOptions = new TransferCopyOptions {
                WriteOptions = new TransferWriteOptions { Mode = flags.Contains("--overwrite")
                    ? TransferWriteMode.Overwrite : flags.Contains("--skip-existing")
                        ? TransferWriteMode.SkipIfExists : TransferWriteMode.FailIfExists },
                VerifyDestination = flags.Contains("--verify")
            }
        };
        TransferReceipt result = await TransferEngine.CopyResumableAsync(source.Endpoint, args[2],
            destination.Endpoint, args[4], options).ConfigureAwait(false);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{result.Outcome}: {result.BytesTransferred} bytes; SHA-256={result.Sha256 ?? "unavailable"}; verified={result.DestinationVerified}"));
        return 0;
    }

    private static async Task<int> ListAsync(string[] args) {
        if (args.Length is < 2 or > 3) { return UsageError("List needs an endpoint URI and optional prefix."); }
        using CliEndpoint endpoint = CliEndpoint.Open(args[1]);
        IReadOnlyList<TransferItem> items = await endpoint.Endpoint.ListAsync(args.Length == 3 ? args[2] : string.Empty)
            .ConfigureAwait(false);
        foreach (TransferItem item in items) {
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{item.Length?.ToString() ?? "?"}\t{item.Path}"));
        }
        return 0;
    }

    private static async Task<int> InspectAsync(string[] args) {
        if (args.Length != 3) { return UsageError("Inspect needs an endpoint URI and item path."); }
        using CliEndpoint endpoint = CliEndpoint.Open(args[1]);
        TransferItem? item = await endpoint.Endpoint.GetItemAsync(args[2]).ConfigureAwait(false);
        if (item == null) { Console.Error.WriteLine("Item not found."); return 3; }
        Console.WriteLine($"Path: {item.Path}");
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Length: {item.Length?.ToString() ?? "unknown"}"));
        Console.WriteLine($"Last modified: {item.LastModifiedUtc?.ToString("O") ?? "unknown"}");
        Console.WriteLine($"ETag: {item.ETag ?? "unknown"}");
        return 0;
    }

    private static async Task<int> SyncAsync(string[] args) {
        if (args.Length < 5) { return UsageError("Sync needs two endpoint URIs and two prefixes."); }
        HashSet<string> flags = ParseFlags(args, 5, "--mirror", "--dry-run", "--no-overwrite", "--allow-empty-source");
        using CliEndpoint source = CliEndpoint.Open(args[1]);
        using CliEndpoint destination = CliEndpoint.Open(args[3]);
        TransferettoSyncOptions options = new() {
            Mode = flags.Contains("--mirror") ? TransferettoSyncMode.Mirror : TransferettoSyncMode.Update,
            DryRun = flags.Contains("--dry-run"),
            OverwriteExisting = !flags.Contains("--no-overwrite")
        };
        TransferettoEndpointSyncResult result = await TransferettoEndpointSync.SyncAsync(source.Endpoint, args[2],
            destination.Endpoint, args[4], options, flags.Contains("--allow-empty-source")).ConfigureAwait(false);
        foreach (TransferettoEndpointSyncItemResult item in result.Items) {
            Console.WriteLine($"{item.PlanItem.Action}\t{item.PlanItem.RelativePath}\t{(item.Error == null ? "ok" : "failed")}");
        }
        return result.IsSuccess ? 0 : 4;
    }

    private static HashSet<string> ParseFlags(string[] args, int start, params string[] allowed) {
        HashSet<string> permitted = new(allowed, StringComparer.Ordinal);
        HashSet<string> flags = new(StringComparer.Ordinal);
        for (int index = start; index < args.Length; index++) {
            if (!permitted.Contains(args[index]) || !flags.Add(args[index])) {
                throw new ArgumentException("An option is unknown or repeated.");
            }
        }
        return flags;
    }

    private static int UsageError(string message) { Console.Error.WriteLine(message); return 2; }

    private static void PrintUsage() {
        Console.WriteLine("Transferetto: copy, resume, inspect, list, and sync endpoint items.");
        Console.WriteLine("copy <source-uri> <source-path> <destination-uri> <destination-path> [--overwrite|--skip-existing] [--verify] [--server-side]");
        Console.WriteLine("resume <source-uri> <source-path> <destination-uri> <destination-path> <checkpoint-path> [--overwrite|--skip-existing] [--verify]");
        Console.WriteLine("list <endpoint-uri> [prefix]");
        Console.WriteLine("inspect <endpoint-uri> <path>");
        Console.WriteLine("sync <source-uri> <source-prefix> <destination-uri> <destination-prefix> [--mirror] [--dry-run] [--no-overwrite] [--allow-empty-source]");
        Console.WriteLine("Endpoint URIs: file:///absolute/root, s3://bucket/prefix, azureblob://container/prefix, sftp://host/prefix, ftp://host/prefix, ftps://host/prefix.");
        Console.WriteLine("Supply provider credentials and trust settings through TRANSFERETTO_* environment variables; URI credentials are rejected.");
    }
}
