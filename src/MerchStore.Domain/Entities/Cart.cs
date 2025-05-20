using System;
using System.Collections.Generic;

namespace MerchStore.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);

        public void AddItem(Guid productId, int qty, decimal price)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                Items.Add(new CartItem { ProductId = productId, Quantity = qty, Price = price });
            else
                item.Quantity += qty;
        }

        public void RemoveItem(Guid productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) Items.Remove(item);
        }
    }
}
