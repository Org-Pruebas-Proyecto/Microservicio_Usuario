namespace Domain.Events;

public class Rol_Asignado_Event
{
    public string Type => "RolAsignadoEvent";
    public Guid UsuarioId { get; }
    public Guid RolId { get; }
    public Rol_Asignado_Event(Guid usuarioId, Guid rolId)
    {
        UsuarioId = usuarioId;
        RolId = rolId;
    }

}