using Application.Commands;
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

        [HttpGet]
        [Route("ObtenerUsuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            return Ok();
        }
    }
}