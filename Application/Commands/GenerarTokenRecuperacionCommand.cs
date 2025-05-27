using MediatR;

namespace Application.Commands;

public record GenerarTokenRecuperacionCommand(string Correo) : IRequest<bool>;