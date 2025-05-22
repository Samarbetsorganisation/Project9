using MerchStore.Domain.Common;
using System;
using System.Collections.Generic;

namespace MerchStore.Domain.Entities;

public class Cart : Entity<Guid>
{
    // Foreign key property
    public Guid UserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation property for user (EF Core sets this, may be null until loaded)
    public User? User { get; private set; }

    // Backing field for cart items
    private readonly List<CartItem> _cartItems;
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

    // Parameterless constructor for EF Core
    private Cart() : base(Guid.NewGuid())
    {
        _cartItems = new List<CartItem>();
        CreatedAt = DateTime.UtcNow;
    }

    // Public constructor
    public Cart(Guid userId) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        _cartItems = new List<CartItem>();
    }

    // Domain method: Add item or increase quantity if already present
    public void AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        var existing = _cartItems.Find(ci => ci.ProductId == product.Id);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _cartItems.Add(new CartItem(Id, product.Id, quantity));
        }
    }

    // Domain method: Remove item
    public void RemoveItem(Guid productId)
    {
        var item = _cartItems.Find(ci => ci.ProductId == productId);
        if (item is not null)
            _cartItems.Remove(item);
    }

    // Domain method: Update quantity or remove if zero/negative
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        var item = _cartItems.Find(ci => ci.ProductId == productId);
        if (item is null)
            throw new InvalidOperationException("Cart item not found.");
        if (newQuantity <= 0)
            _cartItems.Remove(item);
        else
            item.SetQuantity(newQuantity);
    }

    // Domain method: Clear cart
    public void Clear()
    {
        _cartItems.Clear();
    }
}