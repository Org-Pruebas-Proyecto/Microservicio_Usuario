using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.ValueObjects;

public class Rol_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string Nombre { get; set; }
}