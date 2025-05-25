namespace Domain.Events;

public class UsuarioConfirmadoEvent
{
    public Guid UsuarioId { get; }
 

    public UsuarioConfirmadoEvent(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }
}