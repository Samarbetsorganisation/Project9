using MerchStore.Domain.Entities;

namespace MerchStore.Application.Services.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}