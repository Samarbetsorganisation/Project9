using MerchStore.Application.Models.ExternalApi;
using System.Threading.Tasks;

namespace MerchStore.Application.Services.Interfaces
{
    public interface IExternalProductApiService
    {
        /// <summary>
        /// Logs into the external API.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The login response containing the JWT token, or null if login fails.</returns>
        Task<ExternalApiLoginResponse> LoginAsync(string username, string password);

        /// <summary>
        /// Adds a product with details to the external API.
        /// </summary>
        /// <param name="productDetails">The product details to add.</param>
        /// <param name="jwtToken">The JWT token for authorization.</param>
        /// <returns>The response from the external API after adding the product, or null if it fails.</returns>
        Task<ExternalApiProductDetailsResponse> AddProductWithDetailsAsync(ExternalApiAddProductDetailsRequest productDetails, string jwtToken);

        /// <summary>
        /// Deletes a product from the external API by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to delete.</param>
        /// <param name="jwtToken">The JWT token for authorization.</param>
        /// <returns>True if the product was deleted successfully, false otherwise.</returns>
        Task<bool> DeleteProductAsync(string productId, string jwtToken);

        /// <summary>
        /// Gets product information and reviews from the external API by product ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="jwtToken">The JWT token for authorization.</param>
        /// <returns>The product reviews and stats, or null if not found or an error occurs.</returns>
        Task<ExternalApiProductReviewsResponse> GetProductReviewsAsync(string productId, string jwtToken);
    }
}