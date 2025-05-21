using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace MerchStore.Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDb:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("MongoDB connection string is not configured.");

        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);

        // Allow database name override via config or use what's in the connection string
        var databaseName = configuration["MongoDb:DatabaseName"] ?? mongoUrl.DatabaseName ?? "webshop";
        Database = client.GetDatabase(databaseName);
    }
}