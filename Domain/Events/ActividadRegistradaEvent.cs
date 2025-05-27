using Domain.ValueObjects;

namespace Domain.Events;

public class ActividadRegistradaEvent
{
    public Guid ActividadId { get; }
    public Guid UsuarioId { get; }
    public string TipoAccion { get; }
    public string Detalles { get; }
    public DateTime Fecha { get; }

    public ActividadRegistradaEvent(Actividad actividad)
    {
        ActividadId = actividad.Id;
        UsuarioId = actividad.UsuarioId;
        TipoAccion = actividad.TipoAccion;
        Detalles = actividad.Detalles;
        Fecha = actividad.Fecha;
    }
}