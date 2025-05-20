using System;
using System.Collections.Generic;

namespace MerchStore.Application.Cart
{
    public class CartDto
    {
        public Guid UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
