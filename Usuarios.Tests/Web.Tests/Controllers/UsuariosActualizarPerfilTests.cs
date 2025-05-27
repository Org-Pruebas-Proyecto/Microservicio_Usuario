using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosActualizarPerfilTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosActualizarPerfilTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task ActualizarPerfil_DeberiaRetornar_Ok_CuandoActualizacionEsExitosa()
    {
        var dto = new ActualizarPerfilDto(
            Guid.NewGuid(), "Test", "Test",
            "Test@test.com", "123456789", "Calle Falsa 123"
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<ActualizarPerfilCommand>(), default))
            .ReturnsAsync(true);

        var resultado = await _controller.ActualizarPerfil(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Perfil actualizado", okResult.Value);
    }
}