using Application.Commands;
using Application.DTOs;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] RegistroUsuarioDto dto)
        {
            var command = new CrearUsuarioCommand(
                dto.Nombre,
                dto.Apellido,
                dto.Username,
                dto.Password,
                dto.Correo,
                dto.Telefono,
                dto.Direccion
            );
            var usuarioId = await _mediator.Send(command);
            return CreatedAtAction(nameof(CrearUsuario), new { id = usuarioId });
        }
        [HttpPatch("confirmar")]
        public async Task<IActionResult> ConfirmarCuenta([FromBody] ConfirmarUsuarioDto dto)
        {
            var command = new ConfirmarUsuarioCommand(dto.Email, dto.Codigo);
            var result = await _mediator.Send(command);

            return result ? Ok("Cuenta confirmada")
                : BadRequest("Código inválido o expirado");
        }
        [HttpPatch("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var command = new CambiarPasswordCommand(dto.UsuarioId, dto.PasswordActual, dto.NuevoPassword);
            var result = await _mediator.Send(command);
            return result ? Ok("Contraseña cambiada")
                : BadRequest("Error al cambiar la contraseña");
        }
        [HttpPut("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
        {
            var command = new ActualizarPerfilCommand(
                dto.UsuarioId,
                dto.Nombre,
                dto.Apellido,
                dto.Correo,
                dto.Telefono,
                dto.Direccion
            );
            var result = await _mediator.Send(command);
            return result ? Ok("Perfil actualizado")
                : BadRequest("Error al actualizar el perfil");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(Guid id)
        {
            var usuario = await _mediator.Send(new GetUsuarioByIdQuery(id));
            return usuario != null ? Ok(usuario) : NotFound();
        }
    }
}