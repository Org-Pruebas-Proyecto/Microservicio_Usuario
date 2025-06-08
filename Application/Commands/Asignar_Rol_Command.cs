using MediatR;

namespace Application.Commands;

public record Asignar_Rol_Command(
    Guid UsuarioId,
    Guid RolId
    ): IRequest<Unit>;