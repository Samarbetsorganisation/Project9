using System.Threading.Tasks;
using MerchStore.Domain.Repositories;

namespace MerchStore.Application.Cart
{
    public class AddToCartHandler
    {
        private readonly ICartRepository _repo;

        public AddToCartHandler(ICartRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(AddToCartCommand cmd)
        {
            var cart = await _repo.GetByUserIdAsync(cmd.UserId) ?? new Domain.Entities.Cart { UserId = cmd.UserId };
            cart.AddItem(cmd.ProductId, cmd.Quantity, cmd.Price);
            await _repo.SaveAsync(cart);
        }
    }
}
