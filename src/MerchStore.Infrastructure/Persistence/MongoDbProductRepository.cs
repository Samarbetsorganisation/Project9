using MerchStore.Domain.Entities;
using MerchStore.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MerchStore.Infrastructure.Persistence;

public class MongoDbProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products = null!;

    public MongoDbProductRepository(MongoDbContext context)
    {
        _products = context.Database.GetCollection<Product>("Products");
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _products.Find(_ => true).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Product entity)
    {
        await _products.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(Product entity)
    {
        await _products.ReplaceOneAsync(p => p.Id == entity.Id, entity);
    }

    public async Task RemoveAsync(Product entity)
    {
        await _products.DeleteOneAsync(p => p.Id == entity.Id);
    }
}