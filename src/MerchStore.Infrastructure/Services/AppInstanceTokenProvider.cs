using MerchStore.Application.Configuration;
using MerchStore.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider and CreateScope
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MerchStore.Infrastructure.Services
{
    public class AppInstanceTokenProvider : IExternalApiTokenProvider
    {
        // Remove IExternalProductApiService from constructor
        // private readonly IExternalProductApiService _apiService; 
        private readonly IServiceProvider _serviceProvider; // Inject IServiceProvider
        private readonly ExternalApiSettings _settings;
        private readonly ILogger<AppInstanceTokenProvider> _logger;

        private string _token;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

        public AppInstanceTokenProvider(
            IServiceProvider serviceProvider, // Modified constructor
            IOptions<ExternalApiSettings> settings,
            ILogger<AppInstanceTokenProvider> logger)
        {
            _serviceProvider = serviceProvider; // Store IServiceProvider
            _settings = settings.Value;
            _logger = logger;
            _logger.LogInformation("AppInstanceTokenProvider created. Token will be fetched on first request.");
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                _logger.LogDebug("Reusing existing token for this app instance.");
                return _token;
            }

            await _tokenLock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    _logger.LogDebug("Token was fetched by another request while waiting for lock. Reusing.");
                    return _token;
                }

                _logger.LogInformation("No token for this app instance. Attempting login to Review Service.");

                // Create a scope to resolve the Scoped IExternalProductApiService
                using (var scope = _serviceProvider.CreateScope())
                {
                    var apiService = scope.ServiceProvider.GetRequiredService<IExternalProductApiService>();
                    var loginResponse = await apiService.LoginAsync(_settings.Username, _settings.Password);
                
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        _token = loginResponse.Token;
                        _logger.LogInformation("Successfully logged into Review Service. Token stored for app instance lifetime.");
                        return _token;
                    }
                    else
                    {
                        _logger.LogError("Failed to obtain API token from Review Service after login attempt.");
                        throw new InvalidOperationException("Failed to obtain API token from Review Service.");
                    }
                }
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public void InvalidateToken()
        {
            _tokenLock.Wait();
            try
            {
                _logger.LogInformation("Invalidating Review Service token for this app instance. Next GetTokenAsync will re-fetch.");
                _token = null;
            }
            finally
            {
                _tokenLock.Release();
            }
        }
    }
}