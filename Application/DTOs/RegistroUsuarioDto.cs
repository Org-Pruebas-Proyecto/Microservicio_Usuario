namespace Application.DTOs;

public record RegistroUsuarioDto(
    string Nombre,
    string Apellido,
    string Username,
    string Password,
    string Correo,
    string Telefono,
    string Direccion
    );