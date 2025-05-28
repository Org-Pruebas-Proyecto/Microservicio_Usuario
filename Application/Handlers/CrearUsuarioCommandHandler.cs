using Application.Commands;
using MediatR;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;


namespace Application.Handlers;

public class CrearUsuarioCommandHandler : IRequestHandler<CrearUsuarioCommand, Guid>
{
    private readonly IUsuarioRepository _repository;
    private readonly IUsuarioFactory _factory;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISmtpEmailService _smtpEmailService;
    private readonly IActividadRepository _actividadRepository;

    public CrearUsuarioCommandHandler(
        IUsuarioRepository repository,
        IUsuarioFactory factory,
        IEventPublisher eventPublisher,
        ISmtpEmailService smtpEmailService,
        IActividadRepository actividadRepository)
    {
        _repository = repository;
        _factory = factory;
        _eventPublisher = eventPublisher;
        _smtpEmailService = smtpEmailService;
        _actividadRepository = actividadRepository;
    }

    public async Task<Guid> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
    {
        // Usar Factory para crear el usuario
        var usuario = _factory.CrearUsuario(
            request.Nombre,
            request.Apellido,
            request.Username,
            request.Password,
            request.Correo,
            request.Telefono,
            request.Direccion
        );

        if (usuario == null)
        {
            throw new InvalidOperationException("Error al crear usuario");
        }


        await _repository.AddAsync(usuario);

        // Publicar evento
        var evento = new UsuarioCreadoEvent(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Username,
            usuario.Password,
            usuario.Telefono,
            usuario.Correo,
            usuario.Direccion,
            usuario.CodigoConfirmacion
        );

        _eventPublisher.Publish(
            evento,
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.creado"
        );

        // Enviar correo de confirmación

        try
        {
            await _smtpEmailService.EnviarCorreoConfirmacion(
                usuario.Correo,
                usuario.Nombre,
                usuario.CodigoConfirmacion
            );
        }
        catch (Exception ex)
        {
            // Loggear o manejar la excepción según sea necesario
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
        }

        var actividad = new Actividad(
            usuario.Id,
            "Usuario Registrado",
            "El usuario se registró exitosamente en el sistema."
        );

        // Registrar actividad
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

        return usuario.Id;
    }
}