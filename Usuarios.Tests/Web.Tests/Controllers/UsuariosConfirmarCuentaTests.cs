using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosConfirmarCuentaTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosConfirmarCuentaTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task ConfirmarCuenta_DeberiaRetornar_Ok()
    {
        var dto = new ConfirmarUsuarioDto("Test@test.com", "123456");

        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmarUsuarioCommand>(), default))
            .ReturnsAsync(true);

        var resultado = await _controller.ConfirmarCuenta(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Cuenta confirmada", okResult.Value);
    }
}