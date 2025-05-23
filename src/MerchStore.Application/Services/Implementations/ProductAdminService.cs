using MerchStore.Application.Common.Interfaces;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Domain.Entities;

namespace MerchStore.Application.Services.Implementations;

public class ProductAdminService : IProductAdminService
{
    private readonly IRepositoryManager _repositoryManager;

    public ProductAdminService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
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
        await _repositoryManager.ProductRepository.AddAsync(product);
        await _repositoryManager.UnitOfWork.SaveChangesAsync();
        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var existing = await _repositoryManager.ProductRepository.GetByIdAsync(product.Id);
        if (existing is null) return false;

        // Optionally, use domain logic to update
        existing.UpdateDetails(product.Name, product.Description, product.ImageUrl, product.Category, product.Tags);
        existing.UpdatePrice(product.Price);
        existing.UpdateStock(product.StockQuantity);

        await _repositoryManager.ProductRepository.UpdateAsync(existing);
        await _repositoryManager.UnitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _repositoryManager.ProductRepository.GetByIdAsync(id);
        if (existing is null) return false;

        await _repositoryManager.ProductRepository.RemoveAsync(existing);
        await _repositoryManager.UnitOfWork.SaveChangesAsync();
        return true;
    }
}