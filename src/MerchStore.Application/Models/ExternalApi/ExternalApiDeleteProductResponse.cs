using System.Text.Json.Serialization;

namespace MerchStore.Application.Models.ExternalApi
{
    public class ExternalApiDeleteProductResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        // You might also want to capture error details if the API provides them in a structured way
        [JsonPropertyName("error")]
        public string Error { get; set; } 
    }
}