namespace Domain.Events;

public class PerfilActualizadoEvent
{
    public string Type => "PerfilActualizadoEvent";
    public Guid UsuarioId { get; }
    public string Nombre { get; }
    public string Apellido { get; }
    public string Correo { get; }
    public string Telefono { get; }
    public string Direccion { get; }

    public PerfilActualizadoEvent(Guid usuarioId, string nombre, string apellido, string correo, string telefono, string direccion)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Telefono = telefono;
        Direccion = direccion;
    }

}