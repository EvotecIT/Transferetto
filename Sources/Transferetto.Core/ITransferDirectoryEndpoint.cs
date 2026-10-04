using System;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>Optional operations needed to apply physical directory sync actions safely.</summary>
public interface ITransferDirectoryEndpoint {
    /// <summary>Deletes an empty directory. Never recursively deletes its contents.</summary>
    Task<bool> DeleteEmptyDirectoryAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>Optional operation for preserving file modification times during synchronization.</summary>
public interface ITransferTimestampEndpoint {
    /// <summary>Sets the modification time of a committed file.</summary>
    Task SetLastModifiedUtcAsync(string path, DateTimeOffset timestamp, CancellationToken cancellationToken = default);
}
