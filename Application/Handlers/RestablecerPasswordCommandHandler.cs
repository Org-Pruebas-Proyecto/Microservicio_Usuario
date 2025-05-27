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
        IUsuarioRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(RestablecerPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByTokenRecuperacion(request.Token);

        if (usuario == null ||
            usuario.ExpiracionTokenRecuperacion < DateTime.UtcNow)
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

        await _actividadRepository.RegistrarActividad(new Actividad(
            usuario.Id,
            "Restablecimiento de Contraseña",
            "El usuario restableció su contraseña"
        ));

        return true;
    }
}