using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using MediatR;

namespace Application.Handlers;

public class ActualizarPerfilCommandHandler : IRequestHandler<ActualizarPerfilCommand, bool> 
{
    private readonly IUsuarioRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public ActualizarPerfilCommandHandler(
        IUsuarioRepository repository,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(ActualizarPerfilCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.UsuarioId);

        if (usuario == null)
        {
            return false;
        }

        usuario.ActualizarPerfil(
            request.Nombre,
            request.Apellido,
            request.Correo,
            request.Telefono,
            request.Direccion
        );

        await _repository.UpdateAsync(usuario);

        // Publicar evento para sincronización
        try
        {
            _eventPublisher.Publish(
                new PerfilActualizadoEvent(
                    request.UsuarioId,
                    request.Nombre,
                    request.Apellido,
                    request.Correo,
                    request.Telefono,
                    request.Direccion
                ),
                exchangeName: "usuarios_exchange",
                routingKey: "perfil.actualizado"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al publicar evento: {ex.Message}");
        }

        return true;
    }
}