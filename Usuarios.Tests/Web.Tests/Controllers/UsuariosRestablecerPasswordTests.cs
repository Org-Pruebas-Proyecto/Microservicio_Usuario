using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosRestablecerPasswordTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosRestablecerPasswordTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task RestablecerPassword_DeberiaRetornar_BadRequest_CuandoTokenEsInvalido()
    {
        var dto = new RestablecerPasswordDto("token-invalido", "NewPass123", "NewPass123");

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestablecerPasswordCommand>(), default))
            .ReturnsAsync(false);

        var resultado = await _controller.RestablecerPassword(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Token inválido o expirado", badRequestResult.Value);
    }
}