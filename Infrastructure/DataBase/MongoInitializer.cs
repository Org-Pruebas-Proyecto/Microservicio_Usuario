using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.DataBase;

public class MongoInitializer
{
    private readonly IMongoClient _mongoClient;

    public MongoInitializer(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public void Initialize()
    {
        var database = _mongoClient.GetDatabase("usuarios_db");
        var collection = database.GetCollection<UsuarioMongo>("usuarios");

        // Crear índice único en el campo "Username"
        var indexKeys = Builders<UsuarioMongo>.IndexKeys.Ascending(u => u.Username);
        var indexOptions = new CreateIndexOptions { Unique = true };
        collection.Indexes.CreateOne(new CreateIndexModel<UsuarioMongo>(indexKeys, indexOptions));
    }
}