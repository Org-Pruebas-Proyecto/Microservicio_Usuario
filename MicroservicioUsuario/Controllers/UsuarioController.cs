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
        public async Task<IActionResult> CreateUsuario(CreateUsuarioCommand command)
        {
            var usuarioId = await _mediator.Send(command);
            return CreatedAtAction(nameof(CreateUsuario), new { id = usuarioId });
        }
    }
}