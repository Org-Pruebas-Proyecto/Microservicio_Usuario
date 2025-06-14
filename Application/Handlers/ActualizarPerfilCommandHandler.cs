﻿using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;

namespace Application.Handlers;

public class ActualizarPerfilCommandHandler : IRequestHandler<ActualizarPerfilCommand, bool> 
{
    private readonly IUsuarioRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividadRepository _actividadRepository;
    private readonly IKeycloak_Servicio _keycloak_Servicio;

    public ActualizarPerfilCommandHandler(
        IUsuarioRepository repository,
        IEventPublisher eventPublisher,
        IActividadRepository actividadRepository,
        IKeycloak_Servicio keycloak_Servicio)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
        _keycloak_Servicio = keycloak_Servicio;
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

        // Actualizar usuario en Keycloak
        await _keycloak_Servicio.Actualizar_Usuario_Keycloak(usuario);

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

        var actividad = new Actividad(
            request.UsuarioId,
            "Perfil Actualizado",
            "El usuario ha actualizado su perfil."
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

        return true;
    }
}