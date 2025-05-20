using System.Threading.Tasks;
using MerchStore.Domain.Repositories;

namespace MerchStore.Application.Cart
{
    public class GetCartHandler
    {
        private readonly ICartRepository _repo;

        public GetCartHandler(ICartRepository repo)
        {
            _repo = repo;
        }

        public async Task<CartDto> Handle(GetCartQuery query)
        {
            var cart = await _repo.GetByUserIdAsync(query.UserId);
            if (cart == null) return new CartDto();
            return new CartDto
            {
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList(),
                TotalPrice = cart.TotalPrice
            };
        }
    }
}
