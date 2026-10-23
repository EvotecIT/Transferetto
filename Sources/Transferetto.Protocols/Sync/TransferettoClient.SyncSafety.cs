using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transferetto.Core;

namespace Transferetto;

public static partial class TransferettoClient {
    private static TransferettoSyncOptions ResolveLocalSyncComparison(TransferettoSyncOptions options, string root) {
        return options.Direction == TransferettoSyncDirection.Download
            && options.PathComparison == TransferettoSyncPathComparison.Automatic
            ? options.WithPathComparison(TransferFileSystem.GetPathComparison(root) == StringComparison.OrdinalIgnoreCase
                ? TransferettoSyncPathComparison.OrdinalIgnoreCase : TransferettoSyncPathComparison.Ordinal)
            : options;
    }

    private static void ValidateLocalSyncNames(IEnumerable<TransferettoSyncEntry> entries, string root) {
        TransferPathRegistry paths = new(root);
        foreach (TransferettoSyncEntry entry in entries) { paths.Add(entry.RelativePath); }
    }

    private static void ValidateLocalSyncDeletes(IReadOnlyList<TransferettoSyncPlanItem> plan,
        string root, TransferettoSyncOptions options) {
        if (options.Direction != TransferettoSyncDirection.Download) { return; }
        ValidateSyncDeleteIdentities(plan, item => {
            TransferFileSystem.EnsureNoLinkTraversal(root, item.LocalPath!);
            return TransferFileSystem.GetPathIdentity(item.LocalPath!);
        });
    }

    /// <summary>Prevents a mirror delete from addressing a file copied or retained under another spelling.</summary>
    internal static void ValidateSyncDeleteIdentities(IReadOnlyList<TransferettoSyncPlanItem> plan,
        Func<TransferettoSyncPlanItem, string> identity) {
        HashSet<string> retained = new(plan.Where(item => item.Source?.IsDirectory == false)
            .Select(identity), StringComparer.Ordinal);
        foreach (TransferettoSyncPlanItem item in plan.Where(item => item.Action == TransferettoSyncAction.DeleteLocalFile
            || item.Action == TransferettoSyncAction.DeleteRemoteFile)) {
            if (retained.Contains(identity(item))) {
                throw new InvalidDataException("A mirror delete would remove a file retained or copied by this synchronization.");
            }
        }
    }
}
