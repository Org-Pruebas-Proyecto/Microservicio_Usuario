namespace Application.DTOs;

public record RestablecerPasswordDto(
    string Token,
    string NuevaPassword,
    string ConfirmarPassword
    );