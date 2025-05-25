
namespace Domain.Events;

public class UsuarioConfirmadoEvent
{
    public string Type => "UsuarioConfirmadoEvent";
    public Guid UsuarioId { get; }
    public bool Confirmado { get; }

    public UsuarioConfirmadoEvent(Guid usuarioId, bool confirmado)
    {
        UsuarioId = usuarioId;
        Confirmado = confirmado;
    }
}
