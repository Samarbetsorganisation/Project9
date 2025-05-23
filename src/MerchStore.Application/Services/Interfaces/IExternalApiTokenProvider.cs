namespace MerchStore.Application.Services.Interfaces;

public interface IExternalApiTokenProvider
{
    Task<string> GetTokenAsync();
    void InvalidateToken(); // Allows manual invalidation if you ever need it
}