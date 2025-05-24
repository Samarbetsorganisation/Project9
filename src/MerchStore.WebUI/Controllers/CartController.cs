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
                    new() { ProductName="Premium Subscription", UnitPrice=500.0m, Quantity=3 },
                    new() { ProductName="Administration fee", UnitPrice=1000.0m, Quantity=1 },
                    new() {ProductName="Tip", UnitPrice=120.0m, Quantity=1},
                    new() {ProductName="Tipping fee", UnitPrice=20.0m, Quantity=1}
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
