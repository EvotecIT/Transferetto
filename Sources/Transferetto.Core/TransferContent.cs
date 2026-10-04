using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Transferetto.Core;

/// <summary>
/// Provides provider-neutral helpers for safely streaming transfer content.
/// </summary>
public static class TransferContent {
    /// <summary>
    /// Copies content and verifies its advertised length before returning to the caller.
    /// </summary>
    /// <param name="content">The readable source stream.</param>
    /// <param name="destination">The writable staging stream.</param>
    /// <param name="expectedLength">The expected byte length, or <see langword="null"/> when unknown.</param>
    /// <param name="cancellationToken">The token used to cancel the copy.</param>
    /// <returns>The number of bytes copied.</returns>
    /// <exception cref="EndOfStreamException">The content length differs from <paramref name="expectedLength"/>.</exception>
    public static async Task<long> CopyToAsync(
        Stream content,
        Stream destination,
        long? expectedLength,
        CancellationToken cancellationToken = default) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }
        if (destination == null) {
            throw new ArgumentNullException(nameof(destination));
        }

        long? validatedLength = expectedLength >= 0 ? expectedLength : null;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
        long bytesCopied = 0;
        try {
            while (true) {
#if NET8_0_OR_GREATER
                int read = await content.ReadAsync(buffer.AsMemory(0, 81920), cancellationToken).ConfigureAwait(false);
#else
                int read = await content.ReadAsync(buffer, 0, 81920, cancellationToken).ConfigureAwait(false);
#endif
                if (read == 0) {
                    break;
                }

                long nextBytesCopied = checked(bytesCopied + read);
                if (validatedLength.HasValue && nextBytesCopied > validatedLength.Value) {
                    throw new EndOfStreamException(
                        $"The content produced more than its expected length of {validatedLength.Value} bytes.");
                }

#if NET8_0_OR_GREATER
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
                await destination.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
#endif
                bytesCopied = nextBytesCopied;
            }

            if (validatedLength.HasValue && bytesCopied != validatedLength.Value) {
                throw new EndOfStreamException(
                    $"The content produced {bytesCopied} bytes but its expected length is {validatedLength.Value}.");
            }
            return bytesCopied;
        } finally {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>Fills a bounded staging block, stopping only at end of stream or when the block is full.</summary>
    public static async Task<int> ReadBlockAsync(Stream content, byte[] buffer, int count, CancellationToken cancellationToken = default) {
        if (content == null) { throw new ArgumentNullException(nameof(content)); }
        if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
        if (count < 0 || count > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(count)); }
        int total = 0;
        while (total < count) {
#if NET8_0_OR_GREATER
            int read = await content.ReadAsync(buffer.AsMemory(total, count - total), cancellationToken).ConfigureAwait(false);
#else
            int read = await content.ReadAsync(buffer, total, count - total, cancellationToken).ConfigureAwait(false);
#endif
            if (read == 0) { break; }
            total += read;
        }
        return total;
    }
}
