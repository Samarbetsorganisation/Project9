using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MerchStore.Application.Services.Interfaces;

/// <summary>
/// Abstraction for uploading images to a backing store.
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Uploads an image and returns its public URL.
    /// </summary>
    /// <param name="imageStream">The image stream.</param>
    /// <param name="fileName">The filename to use.</param>
    /// <param name="contentType">The image MIME type.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    );
}