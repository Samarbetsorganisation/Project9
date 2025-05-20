using Microsoft.EntityFrameworkCore;
using MerchStore.Domain.Entities;
using MerchStore.Domain.Repositories;
using MerchStore.Infrastructure.Persistence;

namespace MerchStore.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _db;

        public CartRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Cart> GetByUserIdAsync(Guid userId)
        {
            return await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task SaveAsync(Cart cart)
        {
            var existing = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == cart.UserId);

            if (existing == null)
            {
                _db.Carts.Add(cart);
            }
            else
            {
                // Remove CartItems not in the new cart
                foreach (var item in existing.Items.ToList())
                {
                    if (!cart.Items.Any(i => i.ProductId == item.ProductId))
                        _db.CartItems.Remove(item);
                }

                // Update existing items and add new ones
                foreach (var item in cart.Items)
                {
                    var existingItem = existing.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity = item.Quantity;
                        existingItem.Price = item.Price;
                    }
                    else
                    {
                        existing.Items.Add(new CartItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        });
                    }
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Guid userId, Guid productId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart == null) return;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                _db.CartItems.Remove(item); // Also mark for removal from DbSet
            }
            await _db.SaveChangesAsync();
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart == null) return;

            foreach (var item in cart.Items.ToList())
            {
                _db.CartItems.Remove(item);
            }
            cart.Items.Clear();
            await _db.SaveChangesAsync();
        }
    }
}
