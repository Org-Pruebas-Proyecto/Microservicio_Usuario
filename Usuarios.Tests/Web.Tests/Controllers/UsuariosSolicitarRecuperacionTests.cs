using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosSolicitarRecuperacionTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosSolicitarRecuperacionTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task SolicitarRecuperacion_DeberiaRetornar_BadRequest_CuandoCorreoNoExiste()
    {
        var dto = new SolicitudRecuperacionDto("inexistente@test.com");

        _mediatorMock.Setup(m => m.Send(It.IsAny<GenerarTokenRecuperacionCommand>(), default))
            .ReturnsAsync(false);

        var resultado = await _controller.SolicitarRecuperacion(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Correo no registrado", badRequestResult.Value);
    }
}