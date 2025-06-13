﻿using MediatR;

namespace Application.Commands{

public record CrearUsuarioCommand(
            string Nombre,
            string Apellido,
            string Username,
            string Password,
            string Correo,
            string Telefono,
            string Direccion,
            Guid rol_id
        ) : IRequest<Guid>;
}
