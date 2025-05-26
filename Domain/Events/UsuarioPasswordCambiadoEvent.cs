namespace Domain.Events;

public class UsuarioPasswordCambiadoEvent
{
    public string Type => "UsuarioPasswordCambiadoEvent";
    public Guid UsuarioId { get; }
    public string Password { get; }


    public UsuarioPasswordCambiadoEvent(Guid usuarioId, string password)
    {
        UsuarioId = usuarioId;
        Password = password;
    }
}