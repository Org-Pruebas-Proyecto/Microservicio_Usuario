using Application.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosCrearUsuarioTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosCrearUsuarioTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task CrearUsuario_DeberiaRetornar_CreatedAtAction()
    {
        var dto = new RegistroUsuarioDto(
            "Test", "Test", "Test123", "pass123",
            "Test@test.com", "123456789", "Calle Falsa 123"
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearUsuarioCommand>(), default))
            .ReturnsAsync(Guid.NewGuid());

        var resultado = await _controller.CrearUsuario(dto);

        var accionResult = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.NotNull(accionResult.Value);
    }
}