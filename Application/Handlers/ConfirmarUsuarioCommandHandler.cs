using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;

namespace Application.Handlers;

public class ConfirmarUsuarioCommandHandler : IRequestHandler<ConfirmarUsuarioCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividadRepository _actividadRepository;

    public ConfirmarUsuarioCommandHandler(
        IUsuarioRepository repository,
        IEventPublisher eventPublisher,
        IActividadRepository actividadRepository)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
    }

    public async Task<bool> Handle(ConfirmarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmail(request.Email);

        if (usuario == null ||
            usuario.CodigoConfirmacion != request.Codigo ||
            usuario.FechaExpiracionCodigo < DateTime.UtcNow)
        {
            return false;
        }

        usuario.VerificarCuenta();
        await _repository.UpdateAsync(usuario);

        // Publicar evento de confirmación
        _eventPublisher.Publish(
            new UsuarioConfirmadoEvent(usuario.Id, usuario.Verificado),
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.confirmado"
        );

        // Registrar actividad de confirmación
        await _actividadRepository.RegistrarActividad(new Actividad(
            usuario.Id,
            "Verificacion de Cuenta",
            "El usuario verificó su cuenta"
        ));

        return true;
    }
}