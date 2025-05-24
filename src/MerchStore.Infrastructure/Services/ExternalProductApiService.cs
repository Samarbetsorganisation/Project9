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
using System.Threading; // Required for CancellationTokenSource
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

        // Define the timeout period. 2 seconds as discussed.

        private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromMilliseconds(1000);

        public ExternalProductApiService(IHttpClientFactory httpClientFactory, IOptions<ExternalApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExternalProductApiClient");
            _apiSettings = apiSettings.Value;
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl.TrimEnd('/') + "/");
        }

        public async Task<ExternalApiLoginResponse> LoginAsync(string username, string password)
        {
            // The API doc (1.2) specifies username, password, and authType: "password" in the body for login.
            var requestBody = new 
            { 
                username = username, 
                password = password, 
                authType = "password" 
            }; 
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                // For login, we might not want a very short timeout, or use a different one.
                // Using default HttpClient timeout for login unless specified otherwise.
                HttpResponseMessage response = await _httpClient.PostAsync("auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<ExternalApiLoginResponse>(_jsonSerializerOptions);
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return loginResponse;
                }
                else
                {
                    Console.WriteLine($"Login failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during login: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ExternalApiProductDetailsResponse> AddProductWithDetailsAsync(ExternalApiAddProductDetailsRequest productDetails, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                Console.WriteLine("AddProductWithDetailsAsync: JWT token is missing.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            
            try
            {
                // Using default HttpClient timeout for add product.
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("product", productDetails, _jsonSerializerOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ExternalApiProductDetailsResponse>(_jsonSerializerOptions);
                }
                else
                {
                    Console.WriteLine($"AddProductWithDetailsAsync failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during AddProductWithDetailsAsync: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; 
            }
        }

        public async Task<bool> DeleteProductAsync(string productId, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken) || string.IsNullOrWhiteSpace(productId))
            {
                Console.WriteLine("DeleteProductAsync: JWT token or Product ID is missing.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                 // Using default HttpClient timeout for delete product.
                HttpResponseMessage response = await _httpClient.DeleteAsync($"product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"DeleteProductAsync failed for {productId}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during DeleteProductAsync for {productId}: {ex.Message}");
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null; 
            }
        }

        public async Task<ExternalApiProductReviewsResponse> GetProductReviewsAsync(string productId, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken) || string.IsNullOrWhiteSpace(productId))
            {
                Console.WriteLine("GetProductReviewsAsync: JWT token or Product ID is missing.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            
            // Create a CancellationTokenSource for the 2-second timeout
            using var cts = new CancellationTokenSource(DefaultRequestTimeout);

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"product/{productId}", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ExternalApiProductReviewsResponse>(_jsonSerializerOptions, cts.Token);
                }
                else
                {
                    Console.WriteLine($"GetProductReviewsAsync failed for {productId}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    // No need to specifically check for NotFound here to return null, as any non-success will lead to null.
                    return null;
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
            {
                // This specifically catches the timeout
                Console.WriteLine($"GetProductReviewsAsync timed out for product ID {productId}. {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during GetProductReviewsAsync for {productId}: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}