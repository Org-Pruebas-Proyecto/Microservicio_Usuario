﻿using Application.Commands;
using Application.Interfaces;
using MediatR;
using System.Security;
using Domain.Events;


namespace Application.Handlers;

public class CambiarPasswordCommandHandler : IRequestHandler<CambiarPasswordCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly ISmtpEmailService _smtpEmailService;
    private readonly IEventPublisher _eventPublisher;

    public CambiarPasswordCommandHandler(
        IUsuarioRepository repository,
        ISmtpEmailService smtpEmailService,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _smtpEmailService = smtpEmailService;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.UsuarioId);

        if (usuario == null)
        {
            return false;
        }

        // Validar password actual
        if (usuario.Password!=request.PasswordActual)
            throw new SecurityException("Contraseña actual incorrecta");

        // Actualizar password
        usuario.Password = request.NuevoPassword;
        await _repository.UpdateAsync(usuario);

        // Enviar notificación
        try
        {
            await _smtpEmailService.EnviarNotificacionCambioPassword(
                usuario.Correo,
                usuario.Nombre
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
        }


        // Publicar evento de cambio de contraseña
        _eventPublisher.Publish(
            new UsuarioPasswordCambiadoEvent(usuario.Id, usuario.Password),
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.password.cambiado"
        );

        return true;
    }
}