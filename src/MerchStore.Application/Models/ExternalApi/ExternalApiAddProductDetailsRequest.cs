using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiAddProductDetailsRequest
    {
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "withDetails"; // Fixed for this use case

        [JsonPropertyName("productId")]
        public string ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }
}