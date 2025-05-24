using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MerchStore.Models
{
    public class CheckoutViewModel
    {
        // ----- Shipping / Billing Info -----
        [Required] public string FullName { get; set; } = "";
        [Required] public string Email { get; set; } = "";
        [Required] public string AddressLine1 { get; set; } = "";
        public string AddressLine2 { get; set; } = "";
        [Required] public string City { get; set; } = "";
        [Required] public string PostalCode { get; set; } = "";
        [Required] public string Country { get; set; } = "";

        // ----- Payment Info -----
        [Required] public string CardNumber { get; set; } = "";
        [Required] public string ExpiryMonth { get; set; } = "";
        [Required] public string ExpiryYear { get; set; } = "";
        [Required] public string CVV { get; set; } = "";

        // ----- Order Summary -----
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class ThanksViewModel
    {
        

    }
}
