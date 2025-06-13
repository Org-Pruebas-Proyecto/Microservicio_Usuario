using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;


namespace Application.Handlers;

public class Asignar_Rol_Handler : IRequestHandler<Asignar_Rol_Command, Unit>
{
    private readonly IRol_Repositorio _rol_Repositorio;
    private readonly IActividadRepository _actividadRepositorio;
    private readonly IUsuarioRepository _usuarioRepositorio;
    private readonly IEventPublisher _eventPublisher;
    private readonly IKeycloak_Servicio _keycloakServicio;

    public Asignar_Rol_Handler(
        IRol_Repositorio rol_Repositorio,
        IActividadRepository actividadRepositorio,
        IUsuarioRepository usuarioRepositorio,
        IEventPublisher eventPublisher,
        IKeycloak_Servicio keycloakServicio)
    {
        _rol_Repositorio = rol_Repositorio;
        _actividadRepositorio = actividadRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
        _eventPublisher = eventPublisher;
        _keycloakServicio = keycloakServicio;
    }

    public async Task<Unit> Handle(Asignar_Rol_Command request, CancellationToken cancellationToken)
    {
        var rol = await _rol_Repositorio.GetByIdAsync(request.RolId);
        var usuario = await _usuarioRepositorio.GetByIdAsync(request.UsuarioId);
        usuario.AsignarRol(request.RolId);


        // Asignar rol en Keycloak
        await _keycloakServicio.Asignar_Rol_Usuario_Keycloak(usuario.KeycloakId, rol.Nombre);
        
        

        await _usuarioRepositorio.UpdateAsync(usuario);

        // Publicar evento para sincronización
        try
        {
            _eventPublisher.Publish(
                new Rol_Asignado_Event(
                    request.UsuarioId,
                    request.RolId
                ),
                exchangeName: "usuarios_exchange",
                routingKey: "rol.asignado"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al publicar evento: {ex.Message}");
        }
        //Registrar actividad
        var actividad = new Actividad(
            usuario.Id,
            "Asignación de rol",
            $" Al Usuario {usuario.Username} se asigno el Rol {rol.Nombre}"
        );

        await _actividadRepositorio.RegistrarActividad(actividad);

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

        return Unit.Value;
    }
}