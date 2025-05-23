using MerchStore.Domain.Entities;

namespace MerchStore.Application.Services.Interfaces;

public interface IProductAdminService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
}