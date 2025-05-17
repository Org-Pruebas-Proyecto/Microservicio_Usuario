using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class UsuarioMongo
{
    [BsonId] // Indica que es el identificador del documento
    [BsonRepresentation(BsonType.String)] // Serializa como string (formato UUID estándar)
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Username { get; set; }
    public string Correo { get; set; }
}