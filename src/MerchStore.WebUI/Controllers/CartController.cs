using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;   // Session extension methods
using System.Text.Json;            // or Newtonsoft.Json
using MerchStore.Models;
using System.Linq;
using System.Collections.Generic;

namespace MerchStore.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            var vm = new CartViewModel
            {
                Items = new List<CartItemViewModel>
                {
                    new() { ProductName="🧁 Cupcake", UnitPrice=2.50m, Quantity=3 },
                    new() { ProductName="🍞 Bread Loaf", UnitPrice=1.20m, Quantity=2 },
                    new() { ProductName="𓀐𓂸 Dildo", UnitPrice=500m, Quantity=1 },

                }
            };
            vm.TotalPrice = vm.Items.Sum(i => i.Subtotal);

            // Serialize to JSON string
            var json = JsonSerializer.Serialize(vm.Items);
            HttpContext.Session.SetString("CartItems", json);

            return View(vm);
        }
    }
}
