using MediatR;

namespace Application.Commands;

public record CambiarPasswordCommand(
    Guid UsuarioId,
    string PasswordActual,
    string NuevoPassword
) : IRequest<bool>;