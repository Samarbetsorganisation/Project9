using Azure.Storage.Blobs;
using MerchStore.Application.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MerchStore.Infrastructure.Blob;

/// <summary>
/// Azure Blob Storage implementation of IImageUploadService.
/// </summary>
public class AzureBlobImageUploadService : IImageUploadService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobImageUploadService(IOptions<BlobStorageOptions> options)
    {
        var config = options.Value;
        _containerClient = new BlobContainerClient(config.ConnectionString, config.ContainerName);
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, overwrite: true, cancellationToken: cancellationToken);
        await blobClient.SetHttpHeadersAsync(
            new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType },
            conditions: null,
            cancellationToken: cancellationToken
        );
        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageIdentifier, CancellationToken cancellationToken = default)
    {
        string blobName;

        if (Uri.IsWellFormedUriString(imageIdentifier, UriKind.Absolute))
        {
            try
            {
                Uri uri = new Uri(imageIdentifier);
                // The blob name is the last segment of the URI path.
                // Example: https://<account>.blob.core.windows.net/<container>/<blobName>
                // Segments would be ["/", "<container>/", "<blobName>"] or just ["/", "<blobName>"] if container is root (less common for user-facing URLs)
                // More robustly, if the URL matches the container's URI, we can extract.
                // For simplicity, we take the last segment.
                blobName = uri.Segments.LastOrDefault();
                if (!string.IsNullOrEmpty(blobName))
                {
                    // Decode URL encoding if present (e.g., spaces become %20)
                    blobName = Uri.UnescapeDataString(blobName);
                }
            }
            catch (UriFormatException)
            {
                // If it's not a valid URI, or parsing fails, treat it as a filename.
                // This case might be rare if IsWellFormedUriString passed, but good to be cautious.
                blobName = imageIdentifier;
            }
        }
        else
        {
            // Assume it's a filename
            blobName = imageIdentifier;
        }

        if (string.IsNullOrWhiteSpace(blobName))
        {
            // If blobName could not be determined or is empty, nothing to delete.
            // Log this situation if you have logging in place.
            return;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}