using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiReviewItem
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } // Consider parsing to DateTime if needed

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}