using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Transferetto.Core;

namespace Transferetto.AzureBlob;

/// <summary>
/// Exposes one Azure Blob container prefix as a transfer endpoint.
/// </summary>
public sealed class AzureBlobTransferEndpoint : ITransferEndpoint {
    private readonly BlobContainerClient _container;
    private readonly string _prefix;

    /// <summary>Initializes an endpoint from an Azure Storage connection string.</summary>
    public AzureBlobTransferEndpoint(
        string connectionString,
        string containerName,
        string? prefix = null) {
        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        }
        if (string.IsNullOrWhiteSpace(containerName)) {
            throw new ArgumentException("A container name is required.", nameof(containerName));
        }
        _container = new BlobContainerClient(connectionString, containerName);
        ValidateContainerUri(_container.Uri);
        _prefix = NormalizePrefix(prefix);
    }

    /// <summary>Initializes an endpoint from a container URI containing a SAS token.</summary>
    public AzureBlobTransferEndpoint(Uri containerUri, string? prefix = null) {
        ValidateContainerUri(containerUri);
        _container = new BlobContainerClient(containerUri);
        _prefix = NormalizePrefix(prefix);
    }

    /// <summary>Initializes an endpoint from a container URI and shared-key credential.</summary>
    public AzureBlobTransferEndpoint(
        Uri containerUri,
        StorageSharedKeyCredential credential,
        string? prefix = null) {
        ValidateContainerUri(containerUri);
        _container = new BlobContainerClient(
            containerUri,
            credential ?? throw new ArgumentNullException(nameof(credential)));
        _prefix = NormalizePrefix(prefix);
    }

    /// <summary>Initializes an endpoint from a container URI and Azure token credential.</summary>
    public AzureBlobTransferEndpoint(
        Uri containerUri,
        TokenCredential credential,
        string? prefix = null) {
        ValidateContainerUri(containerUri);
        _container = new BlobContainerClient(
            containerUri,
            credential ?? throw new ArgumentNullException(nameof(credential)));
        _prefix = NormalizePrefix(prefix);
    }

    /// <summary>Initializes an endpoint from a container URI and separately protected SAS credential.</summary>
    public AzureBlobTransferEndpoint(
        Uri containerUri,
        AzureSasCredential credential,
        string? prefix = null) {
        ValidateContainerUri(containerUri);
        _container = new BlobContainerClient(
            containerUri,
            credential ?? throw new ArgumentNullException(nameof(credential)));
        _prefix = NormalizePrefix(prefix);
    }

    /// <summary>Initializes an endpoint with a caller-owned Azure SDK client.</summary>
    public AzureBlobTransferEndpoint(BlobContainerClient container, string? prefix = null) {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        ValidateContainerUri(_container.Uri);
        _prefix = NormalizePrefix(prefix);
    }

    /// <inheritdoc />
    public string Scheme => "azureblob";

    /// <inheritdoc />
    public string DisplayName => $"{_container.Uri.GetLeftPart(UriPartial.Path).TrimEnd('/')}/{_prefix}";

    /// <inheritdoc />
    public TransferEndpointCapabilities Capabilities =>
        TransferEndpointCapabilities.Inspect |
        TransferEndpointCapabilities.List |
        TransferEndpointCapabilities.Read |
        TransferEndpointCapabilities.Write |
        TransferEndpointCapabilities.Delete |
        TransferEndpointCapabilities.Metadata |
        TransferEndpointCapabilities.Versioning;

    /// <inheritdoc />
    public async Task<TransferItem?> GetItemAsync(string path, CancellationToken cancellationToken = default) {
        BlobClient blob = _container.GetBlobClient(ResolveName(path));
        try {
            Response<BlobProperties> response = await blob.GetPropertiesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return ToItem(path, response.Value);
        } catch (RequestFailedException exception) when (exception.Status == 404) {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransferItem>> ListAsync(
        string prefix,
        bool recursive = true,
        CancellationToken cancellationToken = default) {
        string resolvedPrefix = ResolveName(prefix, allowEmpty: true);
        List<TransferItem> items = new();
        if (recursive) {
            await foreach (BlobItem item in _container.GetBlobsAsync(
                BlobTraits.Metadata,
                BlobStates.None,
                resolvedPrefix,
                cancellationToken)) {
                items.Add(ToItem(item));
            }
        } else {
            await foreach (BlobHierarchyItem hierarchyItem in _container.GetBlobsByHierarchyAsync(
                BlobTraits.Metadata,
                BlobStates.None,
                "/",
                resolvedPrefix,
                cancellationToken)) {
                if (hierarchyItem.IsBlob) {
                    items.Add(ToItem(hierarchyItem.Blob));
                }
            }
        }
        return items.OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
    }

    /// <inheritdoc />
    public async Task<TransferReadHandle> OpenReadAsync(
        string path,
        CancellationToken cancellationToken = default) {
        BlobClient blob = _container.GetBlobClient(ResolveName(path));
        Response<BlobProperties> properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        Stream stream = await blob.OpenReadAsync(
            CreateOpenReadOptions(properties.Value.ETag),
            cancellationToken).ConfigureAwait(false);
        return new TransferReadHandle(ToItem(path, properties.Value), stream);
    }

    /// <inheritdoc />
    public async Task<TransferWriteResult> WriteAsync(
        string path,
        Stream content,
        long? length,
        TransferWriteOptions? options = null,
        CancellationToken cancellationToken = default) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        TransferWriteOptions resolvedOptions = options ?? new TransferWriteOptions();
        BlobClient blob = _container.GetBlobClient(ResolveName(path));
        if (resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
            TransferItem? existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
        }

        Dictionary<string, string> metadata = TransferMetadata.CopyPortable(resolvedOptions.Metadata);
        BlobUploadOptions uploadOptions = new() {
            HttpHeaders = string.IsNullOrWhiteSpace(resolvedOptions.ContentType)
                ? null
                : new BlobHttpHeaders { ContentType = resolvedOptions.ContentType },
            Metadata = metadata,
            Conditions = resolvedOptions.Mode != TransferWriteMode.Overwrite
                ? new BlobRequestConditions { IfNoneMatch = ETag.All }
                : null
        };
        Response<BlobContentInfo> response;
        using TransferReadTrackingStream trackedContent = new(content, leaveOpen: true);
        try {
            response = await blob.UploadAsync(trackedContent, uploadOptions, cancellationToken).ConfigureAwait(false);
        } catch (RequestFailedException exception) when (
            (exception.Status == 409 || exception.Status == 412) &&
            resolvedOptions.Mode == TransferWriteMode.SkipIfExists) {
            TransferItem? existing = await GetItemAsync(path, cancellationToken).ConfigureAwait(false);
            if (existing != null) {
                return new TransferWriteResult(existing, wasWritten: false);
            }
            throw;
        } catch (RequestFailedException exception) when (
            (exception.Status == 409 || exception.Status == 412) &&
            resolvedOptions.Mode == TransferWriteMode.FailIfExists) {
            throw new IOException($"The destination blob already exists: {path}", exception);
        }
        return new TransferWriteResult(new TransferItem {
            Path = path,
            Length = trackedContent.BytesRead,
            LastModifiedUtc = response.Value.LastModified,
            ETag = response.Value.ETag.ToString().Trim('"'),
            VersionId = response.Value.VersionId,
            ContentType = resolvedOptions.ContentType,
            Metadata = metadata
        }, wasWritten: true);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default) {
        Response<bool> response = await _container.GetBlobClient(ResolveName(path))
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.Value;
    }

    private TransferItem ToItem(BlobItem item) => new() {
        Path = ToRelativeName(item.Name),
        Length = item.Properties.ContentLength,
        LastModifiedUtc = item.Properties.LastModified,
        ETag = item.Properties.ETag?.ToString().Trim('"'),
        VersionId = item.VersionId,
        ContentType = item.Properties.ContentType,
        Metadata = item.Metadata == null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(item.Metadata, StringComparer.OrdinalIgnoreCase)
    };

    private static TransferItem ToItem(string path, BlobProperties properties) => new() {
        Path = path,
        Length = properties.ContentLength,
        LastModifiedUtc = properties.LastModified,
        ETag = properties.ETag.ToString().Trim('"'),
        VersionId = properties.VersionId,
        ContentType = properties.ContentType,
        Metadata = new Dictionary<string, string>(properties.Metadata, StringComparer.OrdinalIgnoreCase)
    };

    private static BlobOpenReadOptions CreateOpenReadOptions(ETag eTag) =>
        new(allowModifications: false) {
            Conditions = new BlobRequestConditions { IfMatch = eTag }
        };

    private string ResolveName(string path, bool allowEmpty = false) {
        string normalized = (path ?? string.Empty).Replace('\\', '/').TrimStart('/');
        if (!allowEmpty && string.IsNullOrEmpty(normalized)) {
            throw new ArgumentException("An endpoint-relative blob name is required.", nameof(path));
        }
        if (normalized.Split('/').Any(segment => segment == "..")) {
            throw new ArgumentException("Parent path segments are not allowed.", nameof(path));
        }
        return _prefix + normalized;
    }

    private string ToRelativeName(string name) =>
        name.StartsWith(_prefix, StringComparison.Ordinal) ? name.Substring(_prefix.Length) : name;

    private static string NormalizePrefix(string? prefix) {
        string normalized = (prefix ?? string.Empty).Replace('\\', '/').Trim('/');
        return string.IsNullOrEmpty(normalized) ? string.Empty : normalized + "/";
    }

    private static void ValidateContainerUri(Uri? containerUri) {
        if (containerUri == null) {
            throw new ArgumentNullException(nameof(containerUri));
        }
        if (!containerUri.IsAbsoluteUri) {
            throw new ArgumentException("The container URI must be absolute.", nameof(containerUri));
        }
        if (!string.IsNullOrEmpty(containerUri.UserInfo)) {
            throw new ArgumentException(
                "Container URIs must not contain user information.",
                nameof(containerUri));
        }
        if (containerUri.Scheme != Uri.UriSchemeHttps && !containerUri.IsLoopback) {
            throw new ArgumentException(
                "Container URIs must use HTTPS unless they are loopback test endpoints.",
                nameof(containerUri));
        }
    }
}
