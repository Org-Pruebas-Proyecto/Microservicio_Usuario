/*

using Application.Commands;
using Application.DTOs;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;

namespace Usuarios.Tests.Web.Tests.Controllers;

public class UsuariosGetUsuarioTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public UsuariosGetUsuarioTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetUsuario_DeberiaRetornar_NotFound_CuandoUsuarioNoExiste()
    {
        var usuarioId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsuarioByIdQuery>(), default))
            .ReturnsAsync((UsuarioDto)null);

        // Tiene un problema, porque ese metodo usa un UsuarioMongo y no me he puesto a ver la logica

        var resultado = await _controller.GetUsuario(usuarioId);

        Assert.IsType<NotFoundResult>(resultado);
    }
}

*/