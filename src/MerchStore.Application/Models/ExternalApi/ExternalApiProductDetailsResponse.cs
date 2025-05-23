using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiProductDetailsResponse
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; } // API returns tags as a single string
    }
}