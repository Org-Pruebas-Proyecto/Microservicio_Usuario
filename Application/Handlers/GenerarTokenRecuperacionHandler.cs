using Application.Commands;
using Application.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Handlers;

public class GenerarTokenRecuperacionHandler : IRequestHandler<GenerarTokenRecuperacionCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly ISmtpEmailService _smtpEmailService;
    private readonly IActividadRepository _actividadRepository;

    public GenerarTokenRecuperacionHandler(
        IUsuarioRepository repository,
        ISmtpEmailService smtpEmailService,
        IActividadRepository actividadRepository)
    {
        _repository = repository;
        _smtpEmailService = smtpEmailService;
        _actividadRepository = actividadRepository;
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

        await _actividadRepository.RegistrarActividad(new Actividad(
            usuario.Id,
            "Generación de Token de Recuperación",
            "El usuario solicitó un token de recuperación de contraseña"
        ));
        return true;
    }
}