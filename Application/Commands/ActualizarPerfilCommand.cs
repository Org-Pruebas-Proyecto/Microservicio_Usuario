using MediatR;

namespace Application.Commands;

public record ActualizarPerfilCommand(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Correo,
    string Telefono,
    string Direccion
    ) : IRequest<bool>;