using Application.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class MongoUsuarioRepository<TDocument> : IMongoUsuarioRepository<TDocument> where TDocument : class
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoUsuarioRepository(IMongoClient mongoClient, string databaseName, string collectionName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<TDocument>(collectionName);
    }

    public override async Task<TDocument> GetByIdAsync(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    
}