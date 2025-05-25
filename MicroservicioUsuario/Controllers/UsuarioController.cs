using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.CrearUsuarioCommand;


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
        public async Task<IActionResult> CrearUsuario(CrearUsuarioCommand command)
        {
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
    }
}