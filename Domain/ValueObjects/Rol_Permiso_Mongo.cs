using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.ValueObjects;

public class Rol_Permiso_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid RolId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid PermisoId { get; set; }
}