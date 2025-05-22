using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MerchStore.Domain.Interfaces;
using MerchStore.Infrastructure.External.ReviewApi;


namespace Project9.Infrastructure.External.ReviewApi
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly HttpClient _httpClient;

        public ProductReviewRepository(HttpClient httpClient, string jwtToken)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://aireviews.drillbi.se");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        public async Task AddProductWithDetailsAsync(string productId, string productName, string category, IEnumerable<string> tags)
        {
            var dto = new ProductWithDetailsRequestDto
            {
                ProductId = productId,
                ProductName = productName,
                Category = category,
                Tags = tags.ToList()
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/product", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductAsync(string productId)
        {
            var response = await _httpClient.DeleteAsync($"/products?productId={productId}");
            response.EnsureSuccessStatusCode();
        }
    }
}