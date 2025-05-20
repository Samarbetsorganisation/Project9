using System;

namespace MerchStore.Application.Cart
{
    public class AddToCartCommand
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // In real app, get price from DB
    }
}
