using MediatR;

namespace Application.Commands;

public record ConfirmarUsuarioCommand(string Email, string Codigo) : IRequest<bool>;