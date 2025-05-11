using MediatR;

namespace Application.Commands{

public record CrearUsuarioCommand(
            string Nombre,
            string Username,
            string Password,
            string Correo,
            string Telefono
        ) : IRequest<Guid>;
}
