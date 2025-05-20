using System.Collections.Generic;

namespace MerchStore.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class CartItemViewModel
    {
        public string  ProductName { get; set; } = "";
        public string  ImageUrl    { get; set; } = "";
        public decimal UnitPrice   { get; set; }
        public int     Quantity    { get; set; }
        public decimal Subtotal    => UnitPrice * Quantity;
    }
}
