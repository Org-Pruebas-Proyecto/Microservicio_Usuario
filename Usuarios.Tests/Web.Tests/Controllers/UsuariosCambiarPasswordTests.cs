using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosCambiarPasswordTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosCambiarPasswordTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task CambiarPassword_DeberiaRetornar_BadRequest_SiPasswordActualEsIncorrecto()
    {
        var dto = new CambiarPasswordDto(Guid.NewGuid(), "wrong123", "newPass123", "newPass123");

        _mediatorMock.Setup(m => m.Send(It.IsAny<CambiarPasswordCommand>(), default))
            .ReturnsAsync(false);

        var resultado = await _controller.CambiarPassword(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Error al cambiar la contraseña", badRequestResult.Value);
    }
}