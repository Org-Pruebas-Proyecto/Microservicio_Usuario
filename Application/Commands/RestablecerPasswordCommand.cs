using MediatR;

namespace Application.Commands;

public record RestablecerPasswordCommand(
    string Token,
    string NuevaPassword
    ) : IRequest<bool>;