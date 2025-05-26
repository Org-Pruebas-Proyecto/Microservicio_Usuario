namespace Application.DTOs;

public record CambiarPasswordDto(
    Guid UsuarioId,
    string PasswordActual,
    string NuevoPassword,
    string ConfirmarPassword
    );