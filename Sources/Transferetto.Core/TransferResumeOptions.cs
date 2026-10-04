using System;

namespace Transferetto.Core;

/// <summary>Controls a restartable ranged copy into a local filesystem endpoint.</summary>
public sealed class TransferResumeOptions {
    /// <summary>Gets or sets the caller-owned path for the durable transfer checkpoint.</summary>
    public string CheckpointPath { get; set; } = string.Empty;
    /// <summary>Gets or sets the staged range size. Defaults to 8 MiB.</summary>
    public int ChunkBytes { get; set; } = 8 * 1024 * 1024;
    /// <summary>Gets or sets the normal copy policy, digest, progress, and readback settings.</summary>
    public TransferCopyOptions CopyOptions { get; set; } = new();
}
