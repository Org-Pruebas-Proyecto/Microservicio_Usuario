using Application.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : class
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(IMongoClient mongoClient, string databaseName, string collectionName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<TDocument>(collectionName);
    }

    public async Task<TDocument> GetByIdAsync(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter)
    {
        var result = await _collection.Find(filter).ToListAsync();
        return result;
    }

}