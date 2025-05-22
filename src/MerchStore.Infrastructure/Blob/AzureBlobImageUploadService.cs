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
}