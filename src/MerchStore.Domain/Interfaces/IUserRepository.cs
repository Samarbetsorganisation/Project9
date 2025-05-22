using MerchStore.Domain.Entities;

namespace MerchStore.Domain.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{

    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    
    // You can add more user-specific methods here if needed
}