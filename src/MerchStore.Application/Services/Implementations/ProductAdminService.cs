using MerchStore.Application.Common.Interfaces;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Domain.Entities;
using MerchStore.Application.Models.ExternalApi; // Needed for ExternalApiAddProductDetailsRequest
using Microsoft.Extensions.Logging; // For logging
using System;
using System.Collections.Generic;
using System.Linq; // For .ToList() if needed for tags
using System.Threading.Tasks;

namespace MerchStore.Application.Services.Implementations
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IExternalProductApiService _externalProductApiService;
        private readonly IExternalApiTokenProvider _tokenProvider;
        private readonly ILogger<ProductAdminService> _logger;

        public ProductAdminService(
            IRepositoryManager repositoryManager,
            IExternalProductApiService externalProductApiService,
            IExternalApiTokenProvider tokenProvider,
            ILogger<ProductAdminService> logger)
        {
            _repositoryManager = repositoryManager;
            _externalProductApiService = externalProductApiService;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repositoryManager.ProductRepository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _repositoryManager.ProductRepository.GetByIdAsync(id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            // 1. Create product in local database
            await _repositoryManager.ProductRepository.AddAsync(product);
            await _repositoryManager.UnitOfWork.SaveChangesAsync();
            _logger.LogInformation("Product {ProductId} created successfully in local database.", product.Id);

            // 2. Attempt to add the product to the external review service
            try
            {
                string token = await _tokenProvider.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError("Failed to obtain API token. Product {ProductId} will not be added to external service.", product.Id);
                    return product;
                }

                var externalProductRequest = new ExternalApiAddProductDetailsRequest
                {
                    ProductId = product.Id.ToString(),
                    ProductName = product.Name,
                    Category = product.Category,
                    Tags = product.Tags?.ToList() ?? new List<string>()
                };

                _logger.LogInformation("Attempting to add product {ProductId} ({ProductName}) to external review service.",
                    externalProductRequest.ProductId, externalProductRequest.ProductName);

                var externalApiResponse = await _externalProductApiService.AddProductWithDetailsAsync(externalProductRequest, token);

                if (externalApiResponse != null)
                {
                    _logger.LogInformation(
                        "Product {ExternalProductId} ({ExternalProductName}) successfully added/updated in external review service.",
                        externalApiResponse.ProductId,
                        externalApiResponse.ProductName);
                }
                else
                {
                    _logger.LogError(
                        "Failed to add product {ProductId} ({ProductName}) to external review service. The external API call returned null or indicated failure.",
                        externalProductRequest.ProductId,
                        externalProductRequest.ProductName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to add product {ProductId} to the external review service.", product.Id);
            }

            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            // 1. Update product in local database
            var existingProduct = await _repositoryManager.ProductRepository.GetByIdAsync(product.Id);
            if (existingProduct is null)
            {
                _logger.LogWarning("Attempted to update non-existent product {ProductId}.", product.Id);
                return false; // Product not found locally
            }

            // Apply updates from the input 'product' DTO/entity to the 'existingProduct'
            existingProduct.UpdateDetails(product.Name, product.Description, product.ImageUrl, product.Category, product.Tags);
            existingProduct.UpdatePrice(product.Price);
            existingProduct.UpdateStock(product.StockQuantity);
            // Add any other update logic here as needed

            await _repositoryManager.ProductRepository.UpdateAsync(existingProduct);
            await _repositoryManager.UnitOfWork.SaveChangesAsync();
            _logger.LogInformation("Product {ProductId} updated successfully in local database.", product.Id);

            // 2. Sync with external service: Delete old then Add new
            try
            {
                string token = await _tokenProvider.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError("Failed to obtain API token. External sync for updated product {ProductId} will be skipped.", product.Id);
                    return true; // Local update succeeded
                }

                // Attempt to delete the old version from the external service
                // Use existingProduct.Id as product.Id might be from a less trusted source or different context
                string productIdString = existingProduct.Id.ToString();
                _logger.LogInformation("Attempting to delete old version of product {ProductId} from external review service before re-adding.", productIdString);
                
                bool deletedExternally = await _externalProductApiService.DeleteProductAsync(productIdString, token);
                if (deletedExternally)
                {
                    _logger.LogInformation("Old version of product {ProductId} deleted successfully from external service.", productIdString);
                }
                else
                {
                    // This could be because it didn't exist (e.g., first time sync after an update) or an error.
                    // ExternalProductApiService.DeleteProductAsync logs specifics.
                    _logger.LogWarning("Failed to delete old version of product {ProductId} from external service, or it was not found. Proceeding to add updated version.", productIdString);
                }

                // Attempt to add the new (updated) version to the external service
                // Use the details from 'existingProduct' as it's the definitive version after local update
                var externalProductRequest = new ExternalApiAddProductDetailsRequest
                {
                    ProductId = productIdString,
                    ProductName = existingProduct.Name,
                    Category = existingProduct.Category,
                    Tags = existingProduct.Tags?.ToList() ?? new List<string>()
                };

                _logger.LogInformation("Attempting to add updated product {ProductId} ({ProductName}) to external review service.",
                    externalProductRequest.ProductId, externalProductRequest.ProductName);

                var externalApiResponse = await _externalProductApiService.AddProductWithDetailsAsync(externalProductRequest, token);

                if (externalApiResponse != null)
                {
                    _logger.LogInformation(
                        "Updated product {ExternalProductId} ({ExternalProductName}) successfully added to external review service.",
                        externalApiResponse.ProductId, // This is the ID returned by the external service
                        externalApiResponse.ProductName);
                }
                else
                {
                    _logger.LogError(
                        "Failed to add updated product {ProductId} ({ProductName}) to external review service after delete. The external API call returned null or indicated failure.",
                        externalProductRequest.ProductId,
                        externalProductRequest.ProductName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during external sync for updated product {ProductId}.", product.Id);
                // Local update succeeded, so we still return true.
            }

            return true; // Reflects success of local update
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // 1. Check if product exists locally
            var existing = await _repositoryManager.ProductRepository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Attempted to delete non-existent product {ProductId} from local database.", id);
                return false; // Product not found locally
            }

            // 2. Delete product from local database
            await _repositoryManager.ProductRepository.RemoveAsync(existing);
            await _repositoryManager.UnitOfWork.SaveChangesAsync();
            _logger.LogInformation("Product {ProductId} deleted successfully from local database.", id);

            // 3. Attempt to delete the product from the external review service
            try
            {
                string token = await _tokenProvider.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError("Failed to obtain API token. Product {ProductId} cannot be deleted from external service.", id);
                    return true;
                }

                _logger.LogInformation("Attempting to delete product {ProductId} from external review service.", id);
                bool deletedExternally = await _externalProductApiService.DeleteProductAsync(id.ToString(), token);

                if (deletedExternally)
                {
                    _logger.LogInformation("Product {ProductId} also deleted successfully from external review service.", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete product {ProductId} from external review service, or it was not found there.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to delete product {ProductId} from the external review service.", id);
            }

            return true;
        }
    }
}