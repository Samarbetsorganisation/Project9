using System;
using System.Threading.Tasks;
using MerchStore.Domain.Entities;

namespace MerchStore.Domain.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetByUserIdAsync(Guid userId);
        Task SaveAsync(Cart cart);
        Task RemoveItemAsync(Guid userId, Guid productId);
        Task ClearCartAsync(Guid userId);
    }
}
