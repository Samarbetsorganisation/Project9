using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using MerchStore.Models;
using System.Linq;
using System.Collections.Generic;

namespace MerchStore.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new CheckoutViewModel();
            var json = HttpContext.Session.GetString("CartItems");

            if (!string.IsNullOrEmpty(json))
            {
                vm.Items = JsonSerializer.Deserialize<List<CartItemViewModel>>(json)!
                              ?? new List<CartItemViewModel>();
                vm.TotalPrice = vm.Items.Sum(i => i.Subtotal);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Thanks()
        {
            var vm = new ThanksViewModel();
            return View(vm);
        }
    }
}
