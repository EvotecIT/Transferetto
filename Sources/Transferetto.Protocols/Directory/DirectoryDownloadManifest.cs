using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Transferetto.Core;

namespace Transferetto;

/// <summary>Freezes one validated listing of each directory before any content is received.</summary>
internal static class DirectoryDownloadManifest {
    internal static IReadOnlyList<Member<T>> Build<T>(string remoteRoot, string localRoot,
        Func<string, IReadOnlyList<T>> list, Func<T, string> name, Func<T, bool> directory,
        Func<T, bool> file, CancellationToken cancellationToken) {
        List<Member<T>> members = new();
        TransferPathRegistry paths = new(localRoot);
        Queue<(string Remote, string Relative)> pending = new();
        pending.Enqueue((remoteRoot.TrimEnd('/') + "/", ""));
        while (pending.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();
            (string remote, string parent) = pending.Dequeue();
            foreach (T item in list(remote)) {
                cancellationToken.ThrowIfCancellationRequested();
                string child = name(item);
                if (child == "." || child == ".." || (!directory(item) && !file(item))) { continue; }
                if (child.Length == 0 || child.IndexOf('/') >= 0) {
                    throw new InvalidDataException("A remote directory member must be a single literal component.");
                }
                string relative = parent.Length == 0 ? child : parent + "/" + child;
                string local = paths.Add(relative);
                // Read only the named child of the directory we actually listed. A server's
                // FullName field cannot redirect a read or recursion outside that namespace.
                string childRemote = remote.TrimEnd('/') + "/" + child;
                bool isDirectory = directory(item);
                members.Add(new Member<T>(relative, childRemote, local, isDirectory, item));
                if (isDirectory) { pending.Enqueue((childRemote, relative)); }
            }
        }
        return members;
    }

    internal sealed class Member<T> {
        internal Member(string relative, string remote, string local, bool directory, T metadata) {
            Relative = relative; Remote = remote; Local = local; IsDirectory = directory; Metadata = metadata;
        }
        internal string Relative { get; }
        internal string Remote { get; }
        internal string Local { get; }
        internal bool IsDirectory { get; }
        internal T Metadata { get; }
    }
}
