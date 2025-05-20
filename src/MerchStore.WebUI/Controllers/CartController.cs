using Microsoft.AspNetCore.Mvc;
using MerchStore.Application.Cart;
using MerchStore.Domain.Repositories;

namespace MerchStore.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly AddToCartHandler _addHandler;
        private readonly GetCartHandler _getHandler;

        public CartController(ICartRepository cartRepo)
        {
            _addHandler = new AddToCartHandler(cartRepo);
            _getHandler = new GetCartHandler(cartRepo);
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid userId)
        {
            var cart = await _getHandler.Handle(new GetCartQuery { UserId = userId });
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddToCartCommand cmd)
        {
            await _addHandler.Handle(cmd);
            return RedirectToAction("Index", new { userId = cmd.UserId });
        }
    }
}
