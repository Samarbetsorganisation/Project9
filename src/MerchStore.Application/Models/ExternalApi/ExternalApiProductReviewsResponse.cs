using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiProductReviewsResponse
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } // The main product ID for this response

        [JsonPropertyName("stats")]
        public ExternalApiReviewStats Stats { get; set; }

        [JsonPropertyName("reviews")]
        public List<ExternalApiReviewItem> Reviews { get; set; }
    }
}