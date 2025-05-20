using System;

namespace MerchStore.Domain.Entities
{
    public class CartItem
    {
        public Guid id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
