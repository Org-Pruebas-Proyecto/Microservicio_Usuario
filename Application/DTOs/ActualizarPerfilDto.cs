namespace Application.DTOs;

public record ActualizarPerfilDto(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Correo,
    string Telefono,
    string Direccion
    );