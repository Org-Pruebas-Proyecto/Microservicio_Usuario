using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;

namespace Application.Handlers;

public class GenerarTokenRecuperacionHandler : IRequestHandler<GenerarTokenRecuperacionCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly ISmtpEmailService _smtpEmailService;
    private readonly IActividadRepository _actividadRepository;
    private readonly IEventPublisher _eventPublisher;

    public GenerarTokenRecuperacionHandler(
        IUsuarioRepository repository,
        ISmtpEmailService smtpEmailService,
        IActividadRepository actividadRepository,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _smtpEmailService = smtpEmailService;
        _actividadRepository = actividadRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(GenerarTokenRecuperacionCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmail(request.Correo);
        if (usuario == null) return false;

        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(24));
        await _repository.UpdateAsync(usuario);

        await _smtpEmailService.EnviarEnlaceRecuperacion(
            usuario.Correo,
            usuario.Nombre,
            usuario.TokenRecuperacion
        );

        var actividad = new Actividad(
            usuario.Id,
            "Generación de Token de Recuperación",
            "El usuario solicitó recuperación de contraseña");

        await _actividadRepository.RegistrarActividad(actividad);

        // Publicar evento de actividad registrada (Mongo)
        _eventPublisher.Publish(
            new ActividadRegistradaEvent(
                actividad.Id,
                actividad.UsuarioId,
                actividad.TipoAccion,
                actividad.Detalles,
                actividad.Fecha
            ),
            exchangeName: "usuarios_exchange",
            routingKey: "actividad.registrada"
        );

        return true;
    }
}