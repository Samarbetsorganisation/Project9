using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiReviewStats
    {
        // Note: The API doc shows "productId" here as "1T12345" vs "T12345" in the outer object.
        // Using the name from the doc.
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } 

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("currentAverage")]
        public double CurrentAverage { get; set; }

        [JsonPropertyName("totalReviews")]
        public int TotalReviews { get; set; }

        [JsonPropertyName("lastReviewDate")]
        public string LastReviewDate { get; set; } // Consider parsing to DateTime
    }
}