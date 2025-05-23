using MerchStore.Application.Configuration;
using MerchStore.Application.Models.ExternalApi;
using MerchStore.Application.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // Requires System.Net.Http.Json NuGet package
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MerchStore.Infrastructure.Services
{
    public class ExternalProductApiService : IExternalProductApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalApiSettings _apiSettings;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Good for flexibility with API responses
        };

        public ExternalProductApiService(IHttpClientFactory httpClientFactory, IOptions<ExternalApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExternalProductApiClient");
            _apiSettings = apiSettings.Value;
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl.TrimEnd('/') + "/");
        }

        public async Task<ExternalApiLoginResponse> LoginAsync(string username, string password)
        {
            // The API doc specifies authType: "basic" in the body for login
            var requestBody = new { authType = "basic" }; 
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Basic authentication header
            var basicAuthHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthHeaderValue);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<ExternalApiLoginResponse>(_jsonSerializerOptions);
                    // Clear Basic Auth after login if subsequent requests use Bearer token
                     _httpClient.DefaultRequestHeaders.Authorization = null;
                    return loginResponse;
                }
                else
                {
                    // Log error: response.StatusCode, await response.Content.ReadAsStringAsync()
                    Console.WriteLine($"Login failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception ex
                Console.WriteLine($"Exception during login: {ex.Message}");
                return null;
            }
            finally
            {
                 // Ensure Basic Auth header is cleared if it's not needed for other requests
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ExternalApiProductDetailsResponse> AddProductWithDetailsAsync(ExternalApiAddProductDetailsRequest productDetails, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                // Log error: JWT token is required
                Console.WriteLine("AddProductWithDetailsAsync: JWT token is missing.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("product", productDetails, _jsonSerializerOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ExternalApiProductDetailsResponse>(_jsonSerializerOptions);
                }
                else
                {
                    // Log error
                    Console.WriteLine($"AddProductWithDetailsAsync failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception during AddProductWithDetailsAsync: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; // Clear token after use
            }
        }

        public async Task<bool> DeleteProductAsync(string productId, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken) || string.IsNullOrWhiteSpace(productId))
            {
                // Log error
                Console.WriteLine("DeleteProductAsync: JWT token or Product ID is missing.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    // Optionally, deserialize ExternalApiDeleteProductResponse if you need the message
                    // var deleteResponse = await response.Content.ReadFromJsonAsync<ExternalApiDeleteProductResponse>(_jsonSerializerOptions);
                    // return deleteResponse?.Message == "Product deleted"; // Or just true on success
                    return true;
                }
                else
                {
                    // Log error
                    Console.WriteLine($"DeleteProductAsync failed for {productId}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception during DeleteProductAsync for {productId}: {ex.Message}");
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; // Clear token after use
            }
        }

        public async Task<ExternalApiProductReviewsResponse> GetProductReviewsAsync(string productId, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken) || string.IsNullOrWhiteSpace(productId))
            {
                // Log error
                Console.WriteLine("GetProductReviewsAsync: JWT token or Product ID is missing.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // API doc says GET /product/{productId} for reviews.
                // The example shows query parameters, but the endpoint description does not.
                // Assuming no query parameters needed based on "Get product reviews" endpoint description.
                // If query params like `mode=reviews` were needed, they'd be added to the URL string.
                HttpResponseMessage response = await _httpClient.GetAsync($"product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ExternalApiProductReviewsResponse>(_jsonSerializerOptions);
                }
                else
                {
                    // Log error
                     Console.WriteLine($"GetProductReviewsAsync failed for {productId}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Handle case where product might not exist in external system
                        return null; 
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Exception during GetProductReviewsAsync for {productId}: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; // Clear token after use
            }
        }
    }
}