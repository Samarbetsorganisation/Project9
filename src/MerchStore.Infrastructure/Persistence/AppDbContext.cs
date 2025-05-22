using Microsoft.EntityFrameworkCore;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MerchStore.Infrastructure.Persistence;

/// <summary>
/// The database context that provides access to the database through Entity Framework Core.
/// This is the central class in EF Core and serves as the primary point of interaction with the database.
/// </summary>
public class AppDbContext : DbContext
{
    // DbSets for all aggregate roots/entities
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;

    /// <summary>
    /// Constructor that accepts DbContextOptions, which allows for configuration to be passed in.
    /// This enables different database providers (SQL Server, In-Memory, etc.) to be used with the same context.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// This method is called when the model for a derived context is being created.
    /// It allows for configuration of entities, relationships, and other model-building activities.
    /// </summary>
    /// <param name="modelBuilder">Provides a simple API for configuring the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from the current assembly (for possible IEntityTypeConfiguration<>)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Product: Configure Value Object 'Money' and Tags as a delimited string, with null-safe ValueComparer for Tags
        modelBuilder.Entity<Product>(builder =>
        {
            builder.OwnsOne(p => p.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("Price_Amount");
                price.Property(p => p.Currency).HasColumnName("Price_Currency");
            });

            builder.Property(p => p.Tags)
                .HasConversion(
                    tags => string.Join(',', tags),
                    db => db.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null) ||
                            (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c == null ? new List<string>() : c.ToList()
                    )
                );
        });

        // Cart: One-to-one with User
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CartItem: Many-to-one with Cart, Many-to-one with Product
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId);

        // User: Username is unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}