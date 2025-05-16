using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;

namespace MerchStore.Infrastructure.Persistence;

/// <summary>
/// Class for seeding the database with initial data.
/// This is useful for development, testing, and demos.
/// </summary>
public class AppDbContextSeeder
{
    private readonly ILogger<AppDbContextSeeder> _logger;
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor that accepts the context and a logger
    /// </summary>
    /// <param name="context">The database context to seed</param>
    /// <param name="logger">The logger for logging seed operations</param>
    public AppDbContextSeeder(AppDbContext context, ILogger<AppDbContextSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public virtual async Task SeedAsync()
    {
        try
        {
            // Ensure the database is created (only needed for in-memory database)
            // For SQL Server, you would use migrations instead
            await _context.Database.EnsureCreatedAsync();

            // Seed products if none exist
            await SeedProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    /// <summary>
    /// Seeds the database with sample products
    /// </summary>
    private async Task SeedProductsAsync()
    {
        // Check if we already have products (to avoid duplicate seeding)
        if (!await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Seeding products...");

            // Add sample products
            var products = new List<Product>
            {
                new Product(
                    "Wizza Staff",
                    "Belonged to the Great Wizard Gandalf The White, the most powerful Wizza of the Middle Earth.",
                    // new Uri("https://example.com/images/tshirt.jpg"),
                    new Uri("https://i.ytimg.com/vi/QziFp6dYxgE/maxresdefault.jpg"),
                    Money.FromSEK(500000m),
                    1),

                new Product(
                    "Developer Mugg",
                    "A ceramic mug with a funny programming joke.",
                    // new Uri("https://example.com/images/mug.jpg"),
                    new Uri("https://sw6.elbenwald.de/media/7b/3c/c8/1645792022/E1069436_3.jpg"),
                    Money.FromSEK(149.50m),
                    0),

                new Product(
                    "Laptop Sticker Pack",
                    "A set of 5 programming language stickers for your laptop.",
                    // new Uri("https://example.com/images/stickers.jpg"),
                    new Uri("https://i.etsystatic.com/36460577/r/il/16fc73/4583997989/il_fullxfull.4583997989_ehbk.jpg"),
                    Money.FromSEK(79.99m),
                    200),

                new Product(
                    "Branded Hoodie",
                    "A warm hoodie with the company logo, perfect for cold offices.",
                    // new Uri("https://example.com/images/hoodie.jpg"),
                    new Uri("https://mockup-api.teespring.com/v3/image/mrAPY9OjVLAcIg7t0hROSfS-u6o/800/800.jpg"),
                    Money.FromSEK(499.99m),
                    25)
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product seeding completed successfully.");
        }
        else
        {
            _logger.LogInformation("Database already contains products. Skipping product seed.");
        }
    }
}