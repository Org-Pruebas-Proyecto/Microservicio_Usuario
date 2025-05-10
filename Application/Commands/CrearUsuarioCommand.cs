using MediatR;

namespace Application.Commands
{
    public class CrearUsuarioCommand
    {
        public record CreateUsuarioCommand(
            string Nombre,
            string Username,
            string Password,
            string Correo,
            string Telefono
        ) : IRequest<Guid>;
    }
}
