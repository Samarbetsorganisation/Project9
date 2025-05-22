using MerchStore.Domain.Common;

namespace MerchStore.Domain.Entities;

public class CartItem : Entity<Guid>
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    // Navigation properties (optional, for EF Core)
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private CartItem() : base(Guid.Empty) { } // For EF Core

    public CartItem(Guid cartId, Guid productId, int quantity)
        : base(Guid.NewGuid())
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("CartId cannot be empty.", nameof(cartId));
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    // Add this method
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Increase amount must be greater than zero.", nameof(amount));
        Quantity += amount;
    }

    // And this method
    public void SetQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));
        Quantity = newQuantity;
    }
}