using MerchStore.Application.Services.Interfaces;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MerchStore.Application.Services.Implementations;

/// <summary>
/// Implementation of IImageUploadService that delegates to the infrastructure provider.
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private readonly IImageUploadService _imageUploadProvider;

    /// <summary>
    /// Constructor. Typically the infrastructure implementation is injected here.
    /// </summary>
    /// <param name="imageUploadProvider">A provider that implements IImageUploadService (e.g., AzureBlobImageUploadService).</param>
    public ImageUploadService(IImageUploadService imageUploadProvider)
    {
        _imageUploadProvider = imageUploadProvider;
    }

    /// <inheritdoc/>
    public Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    ) => _imageUploadProvider.UploadImageAsync(imageStream, fileName, contentType, cancellationToken);
}