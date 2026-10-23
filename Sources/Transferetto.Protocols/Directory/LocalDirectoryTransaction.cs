using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transferetto.Core;

namespace Transferetto;

/// <summary>Stages a protocol directory download, then publishes files and mirror deletions with rollback.</summary>
/// <remarks>The caller controls the destination and its ancestors, as required by TransferFileSystem.
/// This is not crash recovery or isolation from concurrent local writers.</remarks>
internal sealed class LocalDirectoryTransaction : IDisposable {
    private readonly string _root;
    private readonly string _scheme;
    private readonly List<Entry> _entries = new();
    private readonly List<Entry> _published = new();
    private readonly List<string> _created = new();
    private readonly Dictionary<string, Times> _previousTimes = new(StringComparer.Ordinal);
    private string? _stage;
    private string? _stageParent;
    private Times? _stageParentTimes;
    private bool _committed;
    private bool _retainRecovery;

    internal LocalDirectoryTransaction(string root, string scheme = "scp") {
        _scheme = scheme;
        _root = Path.GetFullPath(root);
        if (_root.Length > Path.GetPathRoot(_root)!.Length) {
            _root = _root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    internal void AddDirectory(string relative, DateTime? modified, DateTime? accessed) =>
        _entries.Add(new Entry(relative, null, modified, accessed));

    /// <summary>Receives through a path-based transport into private staging; only successful receives are published.</summary>
    internal bool AddFile(string relative, Func<string, bool> receive, bool seedExisting = false,
        CancellationToken cancellationToken = default) {
        string target = Resolve(relative);
        string staged = Path.Combine(GetStage(), Guid.NewGuid().ToString("N"));
        if (seedExisting && File.Exists(target)) {
            // Copy bytes without importing the original's potentially broad access rules
            // into private incoming staging. Publication preserves the real target's rules.
            using FileStream source = new(target, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream destination = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            TransferContent.CopyToAsync(source, destination, source.Length, cancellationToken).GetAwaiter().GetResult();
        }
        if (!receive(staged)) { return false; }
        if (!File.Exists(staged)) { throw new IOException("The directory receive did not produce its staged file."); }
        _entries.Add(new Entry(relative, staged, null, null));
        return true;
    }

    /// <summary>Defers removal of a mirror extra until receipt succeeds; keeps its original for rollback.</summary>
    internal void DeleteFile(string relative) {
        Resolve(relative);
        _entries.Add(new Entry(relative, null, null, null) { IsDeletion = true });
    }

    internal async Task AddFileAsync(string relative, Stream content, long length, DateTime? modified,
        DateTime? accessed, CancellationToken cancellationToken) {
        string target = Resolve(relative);
        string staged = Path.Combine(GetStage(), Guid.NewGuid().ToString("N"));
        using (FileStream output = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true)) {
            TransferFileSystem.PreserveStagingPermissions(staged, target);
            await TransferContent.CopyToAsync(content, output, length, cancellationToken).ConfigureAwait(false);
        }
        _entries.Add(new Entry(relative, staged, modified, accessed));
    }

    internal async Task CommitAsync(CancellationToken cancellationToken) {
        // Check every destination type before publishing anything. Validation always uses the
        // real destination, not the staging directory's possibly different case semantics.
        foreach (Entry entry in _entries) {
            cancellationToken.ThrowIfCancellationRequested();
            string target = Resolve(entry.Relative);
            if (entry.IsDirectory ? File.Exists(target) : Directory.Exists(target)) {
                throw new IOException("A directory member conflicts with an existing destination type: " + entry.Relative);
            }
            if (!entry.IsDirectory && File.Exists(target) && (File.GetAttributes(target) & FileAttributes.ReadOnly) != 0) {
                throw new UnauthorizedAccessException("The directory destination is read-only: " + entry.Relative);
            }
        }
        try {
            foreach (Entry entry in _entries) {
                cancellationToken.ThrowIfCancellationRequested();
                string target = Resolve(entry.Relative);
                if (entry.IsDirectory) {
                    EnsureDirectory(target);
                } else if (entry.IsDeletion) {
                    if (!File.Exists(target)) { continue; }
                    EnsureDirectory(Path.GetDirectoryName(target)!);
                    _published.Add(entry);
                    string backup = Path.Combine(GetStage(), Guid.NewGuid().ToString("N"));
                    File.Copy(target, backup);
                    entry.Backup = backup;
                    TransferFileSystem.PreserveStagingPermissions(backup, target);
                    File.Delete(target);
                    entry.Written = true;
                } else {
                    EnsureDirectory(Path.GetDirectoryName(target)!);
                    _published.Add(entry);
                    // Final staging is adjacent to the destination so a new file inherits
                    // that directory's ACL/default permissions rather than the private store's.
                    string adjacent = Path.Combine(Path.GetDirectoryName(target)!, ".transferetto-" + Guid.NewGuid().ToString("N") + ".tmp");
                    using (FileStream output = new(adjacent, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true)) {
                        entry.PublishStage = adjacent;
                        TransferFileSystem.PreserveStagingPermissions(adjacent, target);
                        using FileStream source = new(entry.Staged!, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                        await TransferContent.CopyToAsync(source, output, source.Length, cancellationToken).ConfigureAwait(false);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    File.Delete(entry.Staged!);
                    if (File.Exists(target)) {
                        entry.Backup = Path.Combine(GetStage(), Guid.NewGuid().ToString("N"));
                        // ReplaceFile preserves destination metadata (including its Windows ACL)
                        // and retains the original for rollback, without first removing the target.
                        File.Replace(adjacent, target, entry.Backup);
                    } else {
                        File.Move(adjacent, target);
                    }
                    entry.Written = true;
                    ApplyTimes(target, entry.Modified, entry.Accessed);
                }
            }
            foreach (Entry directory in _entries.Where(entry => entry.IsDirectory).Reverse()) {
                ApplyTimes(Resolve(directory.Relative), directory.Modified, directory.Accessed);
            }
            cancellationToken.ThrowIfCancellationRequested();
            _committed = true;
        } catch (Exception original) {
            try { Rollback(); }
            catch (Exception recovery) {
                _retainRecovery = true;
                original.Data["Transferetto.RollbackException"] = recovery;
                if (_stage != null) { original.Data["Transferetto.RecoveryDirectory"] = _stage; }
            }
            throw;
        }
    }

    private void Rollback() {
        foreach (Entry entry in _published.AsEnumerable().Reverse()) {
            string target = Resolve(entry.Relative);
            if (entry.Written) { File.Delete(target); }
            if (entry.Backup != null && File.Exists(entry.Backup)) {
                if (entry.IsDeletion && !entry.Written) { File.Delete(entry.Backup); }
                else { File.Move(entry.Backup, target); }
            }
            if (entry.PublishStage != null) {
                TransferFileSystem.EnsureNoLinkTraversal(_root, entry.PublishStage);
                File.Delete(entry.PublishStage);
            }
        }
        foreach (string directory in _created.AsEnumerable().Reverse()) {
            TransferFileSystem.EnsureNoLinkTraversal(directory, directory);
            Directory.Delete(directory, recursive: false);
        }
        foreach (KeyValuePair<string, Times> previous in _previousTimes) { previous.Value.Restore(previous.Key); }
    }

    private void EnsureDirectory(string path) {
        TransferFileSystem.EnsureNoLinkTraversal(path, path);
        if (Directory.Exists(path)) {
            if (!_created.Contains(path) && !_previousTimes.ContainsKey(path)) { _previousTimes.Add(path, new Times(path)); }
            return;
        }
        string? parent = Path.GetDirectoryName(path);
        if (parent == null || parent == path) { throw new DirectoryNotFoundException(path); }
        EnsureDirectory(parent);
        Directory.CreateDirectory(path);
        _created.Add(path);
    }

    private string GetStage() {
        if (_stage != null) { return _stage; }
        // An existing root can itself be a mount point. Stage beneath it so moves
        // and backups remain on that filesystem, irrespective of its parent's volume.
        string parent = Directory.Exists(_root) ? _root : Path.GetDirectoryName(_root) ?? _root;
        while (!Directory.Exists(parent)) {
            if (File.Exists(parent)) { throw new IOException("The directory destination parent is a file."); }
            parent = Path.GetDirectoryName(parent) ?? throw new DirectoryNotFoundException(parent);
        }
        TransferFileSystem.EnsureNoLinkTraversal(parent, parent);
        _stageParent = parent;
        _stageParentTimes = new Times(parent);
        string stage = Path.Combine(parent, ".transferetto-" + _scheme + "-" + Guid.NewGuid().ToString("N"));
        try { TransferFileSystem.CreatePrivateStagingDirectory(stage); }
        catch {
            try { _stageParentTimes.Restore(parent); }
            catch (Exception cleanup) { TransferDiagnostics.RecordCleanupFailure(_scheme, cleanup); }
            throw;
        }
        _stage = stage;
        return _stage;
    }

    private string Resolve(string relative) {
        string target = TransferFileSystem.ResolveRelativePath(_root, relative, allowEmpty: true);
        if (_stage != null) {
            string identity = TransferFileSystem.GetPathIdentity(target);
            string stageIdentity = TransferFileSystem.GetPathIdentity(_stage);
            if (identity == stageIdentity || identity.StartsWith(stageIdentity + Path.DirectorySeparatorChar, StringComparison.Ordinal)) {
                throw new InvalidDataException("A directory member conflicts with the private staging directory.");
            }
        }
        return target;
    }

    private static void ApplyTimes(string path, DateTime? modified, DateTime? accessed) {
        if (Directory.Exists(path)) {
            if (modified.HasValue) { Directory.SetLastWriteTimeUtc(path, modified.Value); }
            if (accessed.HasValue) { Directory.SetLastAccessTimeUtc(path, accessed.Value); }
        } else {
            if (modified.HasValue) { File.SetLastWriteTimeUtc(path, modified.Value); }
            if (accessed.HasValue) { File.SetLastAccessTimeUtc(path, accessed.Value); }
        }
    }

    public void Dispose() {
        if (_stage == null || _retainRecovery) { return; }
        try {
            TransferFileSystem.EnsureNoLinkTraversal(_stage, _stage);
            foreach (string file in Directory.EnumerateFileSystemEntries(_stage)) {
                TransferFileSystem.EnsureNoLinkTraversal(_stage, file);
            }
            Directory.Delete(_stage, recursive: true);
        } catch (Exception cleanup) {
            TransferDiagnostics.RecordCleanupFailure(_scheme, cleanup);
        }
        // Timestamp restoration must still run when stage cleanup fails, and neither
        // cleanup failure may replace a receive error or turn publication into failure.
        try {
            if (!_committed) { _stageParentTimes!.Restore(_stageParent!); }
            else if (_stageParent == _root) {
                Entry root = _entries.First(entry => entry.Relative.Length == 0);
                ApplyTimes(_root, root.Modified, root.Accessed);
            }
        } catch (Exception cleanup) {
            TransferDiagnostics.RecordCleanupFailure(_scheme, cleanup);
        }
    }

    private sealed class Entry {
        internal Entry(string relative, string? staged, DateTime? modified, DateTime? accessed) {
            Relative = relative; Staged = staged; Modified = modified; Accessed = accessed;
        }
        internal string Relative { get; }
        internal string? Staged { get; }
        internal bool IsDeletion { get; set; }
        internal bool IsDirectory => Staged == null && !IsDeletion;
        internal DateTime? Modified { get; }
        internal DateTime? Accessed { get; }
        internal string? Backup { get; set; }
        internal string? PublishStage { get; set; }
        internal bool Written { get; set; }
    }

    private sealed class Times {
        private readonly DateTime _modified;
        private readonly DateTime _accessed;
        internal Times(string path) { _modified = Directory.GetLastWriteTimeUtc(path); _accessed = Directory.GetLastAccessTimeUtc(path); }
        internal void Restore(string path) => ApplyTimes(path, _modified, _accessed);
    }
}
