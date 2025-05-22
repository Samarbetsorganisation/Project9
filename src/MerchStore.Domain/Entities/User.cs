using MerchStore.Domain.Common;

namespace MerchStore.Domain.Entities;

public class User : Entity<Guid>
{
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsAdmin { get; private set; }

    // Navigation property
    public Cart? Cart { get; private set; }

    private User() : base(Guid.Empty) { } // For EF Core

    public User(string username, string passwordHash, bool isAdmin = false)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));
        
        Username = username;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
    }
}