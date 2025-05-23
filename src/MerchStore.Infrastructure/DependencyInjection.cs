using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MerchStore.Application.Common.Interfaces; // Assuming this might be for IUnitOfWork or similar if defined there
using MerchStore.Domain.Interfaces;
using MerchStore.Infrastructure.Persistence;
using MerchStore.Infrastructure.Persistence.Repositories;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Infrastructure.Blob;
using MerchStore.Application.Configuration; // Added for ExternalApiSettings
using MerchStore.Infrastructure.Services; // Added for ExternalProductApiService
using System; // Added for Environment.GetEnvironmentVariable and InvalidOperationException
using System.Threading.Tasks; // Added for Task

namespace MerchStore.Infrastructure;

/// <summary>
/// Contains extension methods for registering Infrastructure layer services with the dependency injection container.
/// This keeps all registration logic in one place and makes it reusable.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the DI container
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">The configuration for database connection strings and other settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQL Server using environment variable from GitHub Secrets
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration["SqlDb:ConnectionString"] // Reads from environment variable
            ));

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repository Manager
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        // === BLOB STORAGE CONFIGURATION ===
        // Manually bind options from environment variables (keeps BLOBCONNECTIONSTRING and BLOBCONTAINERNAME as is)
        services.Configure<BlobStorageOptions>(options =>
        {
            options.ConnectionString = Environment.GetEnvironmentVariable("BLOBCONNECTIONSTRING")
                ?? throw new InvalidOperationException("Environment variable BLOBCONNECTIONSTRING is not set");
            options.ContainerName = Environment.GetEnvironmentVariable("BLOBCONTAINERNAME")
                ?? throw new InvalidOperationException("Environment variable BLOBCONTAINERNAME is not set");
        });

        // Register Blob service
        services.AddScoped<IImageUploadService, AzureBlobImageUploadService>();

        // === EXTERNAL PRODUCT API CONFIGURATION ===
        // Manually bind ExternalApiSettings from environment variables
        services.Configure<ExternalApiSettings>(options =>
        {
            options.BaseUrl = Environment.GetEnvironmentVariable("REVIEWSERVICE__BASEURL")
                ?? throw new InvalidOperationException("Environment variable REVIEWSERVICE__BASEURL is not set");
            options.Username = Environment.GetEnvironmentVariable("REVIEWSERVICE__USERNAME")
                ?? throw new InvalidOperationException("Environment variable REVIEWSERVICE__USERNAME is not set");
            options.Password = Environment.GetEnvironmentVariable("REVIEWSERVICE__PASSWORD")
                ?? throw new InvalidOperationException("Environment variable REVIEWSERVICE__PASSWORD is not set");
        });

        // Configure HttpClient for ExternalProductApiService
        // The name "ExternalProductApiClient" matches the one used in ExternalProductApiService constructor
        services.AddHttpClient("ExternalProductApiClient");
        // If you need to configure the HttpClient further (e.g., with Polly for resilience):
        // services.AddHttpClient("ExternalProductApiClient", client =>
        // {
        //     // client.Timeout = TimeSpan.FromSeconds(30); // Example: Set a timeout
        // })
        // .AddPolicyHandler(GetRetryPolicy()) // Example: Add a Polly retry policy
        // .AddPolicyHandler(GetCircuitBreakerPolicy()); // Example: Add a Polly circuit breaker

        // Register External Product API Service
        services.AddScoped<IExternalProductApiService, ExternalProductApiService>();

                // === TOKEN PROVIDER FOR EXTERNAL API ===
        // Registered as Singleton to maintain the token for the app instance lifetime
        services.AddSingleton<IExternalApiTokenProvider, AppInstanceTokenProvider>();


        // Add logging services if not already added
        services.AddLogging();

        // Register DbContext seeder
        services.AddScoped<AppDbContextSeeder>();

        return services;
    }

    /// <summary>
    /// Seeds the database with initial data.
    /// This is an extension method on IServiceProvider to allow it to be called from Program.cs.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AppDbContextSeeder>();
        await seeder.SeedAsync();
    }

}