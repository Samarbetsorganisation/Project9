using Azure.Storage.Blobs;

namespace MerchStore.Infrastructure.Blob
{
    public class AzureBlobImageUploadService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobImageUploadService(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
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
}