using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;


namespace Application.Handlers;

public class RestablecerPasswordCommandHandler : IRequestHandler<RestablecerPasswordCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividadRepository _actividadRepository;



    public RestablecerPasswordCommandHandler(
        IUsuarioRepository repository, IEventPublisher eventPublisher, IActividadRepository actividadRepository)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
    }

    public async Task<bool> Handle(RestablecerPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByTokenRecuperacion(request.Token);

        if (usuario == null || usuario.ExpiracionTokenRecuperacion == null || usuario.ExpiracionTokenRecuperacion < DateTime.UtcNow)
        {
            return false;
        }


        usuario.ActualizarPassword(request.NuevaPassword);

        await _repository.UpdateAsync(usuario);


        try
        {
            _eventPublisher.Publish(
                new UsuarioPasswordCambiadoEvent(usuario.Id, usuario.Password),
                exchangeName: "usuarios_exchange",
                routingKey: "usuario.password.cambiado"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al publicar evento: {ex.Message}");
        }

        var actividad = new Actividad(
            usuario.Id,
            "Restablecimiento de Contraseña",
            "El usuario ha restablecido su contraseña exitosamente."
        );

        try
        {
            await _actividadRepository.RegistrarActividad(actividad);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar actividad: {ex.Message}");
        }

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