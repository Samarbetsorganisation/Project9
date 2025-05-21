using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MerchStore.Domain.Entities;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.Description)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(p => p.ImageUrl)
               .HasMaxLength(2000);

        builder.Property(p => p.StockQuantity)
               .IsRequired();

        builder.Property(p => p.Category)
               .HasMaxLength(100)
               .IsRequired();

        // Map Money value object as owned type
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("Price_Amount")
                 .HasPrecision(18, 2) // matches DECIMAL(18,2)
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("Price_Currency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        // Map Tags as CSV string in single column
        builder.Property(p => p.Tags)
               .HasConversion(
                   v => string.Join(',', v),
                   v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
               )
               .HasColumnName("Tags")
               .HasMaxLength(1000);
    }
}