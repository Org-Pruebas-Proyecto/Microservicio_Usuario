using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.ValueObjects;

public class ActividadMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string TipoAccion { get; set; }
    public string Detalles { get; set; }
    public DateTime Fecha { get; set; }
}