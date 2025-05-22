using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MerchStore.Application.Common.Interfaces;
using MerchStore.Domain.Interfaces;
using MerchStore.Infrastructure.Persistence;
using MerchStore.Infrastructure.Persistence.Repositories;
using MerchStore.Application.Services.Interfaces;
using MerchStore.Infrastructure.Blob;

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
    /// <param name="configuration">The configuration for database connection strings</param>
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
                ?? throw new InvalidOperationException("BLOBCONNECTIONSTRING is not set");
            options.ContainerName = Environment.GetEnvironmentVariable("BLOBCONTAINERNAME") 
                ?? throw new InvalidOperationException("BLOBCONTAINERNAME is not set");
        });

        // Register Blob service
        services.AddScoped<IImageUploadService, AzureBlobImageUploadService>();

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