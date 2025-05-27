using Application.Commands;
using Application.Interfaces;
using MediatR;

namespace Application.Handlers;

public class GenerarTokenRecuperacionHandler : IRequestHandler<GenerarTokenRecuperacionCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly ISmtpEmailService _smtpEmailService;

    public GenerarTokenRecuperacionHandler(
        IUsuarioRepository repository,
        ISmtpEmailService smtpEmailService)
    {
        _repository = repository;
        _smtpEmailService = smtpEmailService;

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

        return true;
    }
}