namespace MerchStore.Domain.Interfaces
{
    public interface IProductReviewRepository
    {
        Task AddProductWithDetailsAsync(string productId, string productName, string category, IEnumerable<string> tags);
        Task DeleteProductAsync(string productId);
    }
}