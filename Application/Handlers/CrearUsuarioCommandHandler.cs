using MediatR;
using Domain.Entities;
using Application.Interfaces;
using static Application.Commands.CrearUsuarioCommand;


namespace Application.Handlers;

public class CrearUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, Guid>
{
    private readonly IUsuarioRepository _repository;

    public CrearUsuarioCommandHandler(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(
            request.Nombre,
            request.Username,
            request.Password,
            request.Correo,
            request.Telefono
        );

        await _repository.AddAsync(usuario);
        return usuario.Id;
    }
}