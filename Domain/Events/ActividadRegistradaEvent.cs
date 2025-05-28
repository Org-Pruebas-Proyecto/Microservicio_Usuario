using Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Domain.Events;

public class ActividadRegistradaEvent
{
    public string Type => "ActividadRegistradaEvent";
    [JsonPropertyName("actividadId")]
    public Guid ActividadId { get; }

    [JsonPropertyName("usuarioId")]
    public Guid UsuarioId { get; }

    [JsonPropertyName("tipoAccion")]
    public string TipoAccion { get; }

    [JsonPropertyName("detalles")]
    public string Detalles { get; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; }

    // Constructor con nombres de parámetros en camelCase
    [JsonConstructor]
    public ActividadRegistradaEvent(
        Guid actividadId,
        Guid usuarioId,
        string tipoAccion,
        string detalles,
        DateTime fecha)
    {
        ActividadId = actividadId;
        UsuarioId = usuarioId;
        TipoAccion = tipoAccion;
        Detalles = detalles;
        Fecha = fecha;
    }
}


