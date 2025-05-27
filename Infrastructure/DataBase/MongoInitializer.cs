using Domain.Entities;
using Domain.ValueObjects;
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

        // Configurar colección de usuarios
        InitializeUsuarios(database);

        // Configurar colección de actividades
        InitializeActividades(database);
    }

    private void InitializeUsuarios(IMongoDatabase database)
    {
        var collection = database.GetCollection<UsuarioMongo>("usuarios");

        // Índice único para username
        var usuarioIndexKeys = Builders<UsuarioMongo>.IndexKeys
            .Ascending(u => u.Username);

        collection.Indexes.CreateOne(
            new CreateIndexModel<UsuarioMongo>(usuarioIndexKeys,
                new CreateIndexOptions { Unique = true })
        );
    }

    private void InitializeActividades(IMongoDatabase database)
    {
        var collection = database.GetCollection<ActividadMongo>("actividades");

        // Índice compuesto para búsquedas por usuario y fecha
        var actividadIndexKeys = Builders<ActividadMongo>.IndexKeys
            .Ascending(a => a.UsuarioId)
            .Descending(a => a.Fecha);

        collection.Indexes.CreateOne(
            new CreateIndexModel<ActividadMongo>(actividadIndexKeys)
        );

    }
}