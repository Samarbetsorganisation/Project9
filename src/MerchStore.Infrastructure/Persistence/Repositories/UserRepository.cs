using MerchStore.Domain.Entities;
using MerchStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MerchStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing User entities.
/// This class inherits from the generic Repository class and adds user-specific functionality.
/// </summary>
public class UserRepository : Repository<User, Guid>, IUserRepository
{
    /// <summary>
    /// Constructor that passes the context to the base Repository class
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    //Gets user by username
    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    //Checks if user with provided username parameter exists
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    // You can add User-specific methods here if needed
}