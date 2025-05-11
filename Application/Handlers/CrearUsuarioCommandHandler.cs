using Application.Commands;
using MediatR;
using Domain.Entities;
using Application.Interfaces;
using Domain.Events;


namespace Application.Handlers;

public class CrearUsuarioCommandHandler : IRequestHandler<CrearUsuarioCommand, Guid>
{
    private readonly IUsuarioRepository _repository;
    private readonly IUsuarioFactory _factory;
    private readonly IEventPublisher _eventPublisher;

    public CrearUsuarioCommandHandler(
        IUsuarioRepository repository,
        IUsuarioFactory factory,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _factory = factory;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        // Usar Factory para crear el usuario
        var usuario = _factory.CrearUsuario(
            request.Nombre,
            request.Username,
            request.Password,
            request.Correo,
            request.Telefono
        );

        await _repository.AddAsync(usuario);

        // Publicar evento
        var evento = new UsuarioCreadoEvent(
            usuario.Id,
            usuario.Nombre,
            usuario.Username,
            usuario.Correo
        );

        _eventPublisher.Publish(
            evento,
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.creado"
        );

        return usuario.Id;
    }
}