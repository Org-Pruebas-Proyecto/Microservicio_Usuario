using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Queries;
using Domain.Entities;
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
    public async Task GetUsuario_ShouldReturnOk_WhenUsuarioMongoExists()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuarioMongoMock = new UsuarioMongo
        {
            Id = usuarioId,
            Nombre = "Test",
            Apellido = "Ting",
            Username = "testing",
            Password = "pass123",
            Correo = "test@ting.com",
            Telefono = "123456789",
            Direccion = "Some Address",
            Verificado = true
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsuarioByIdQuery>(), default))
            .ReturnsAsync(usuarioMongoMock);

        // Act
        var result = await _controller.GetUsuario(usuarioId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.IsType<UsuarioMongo>(result.Value); // Validación del tipo correcto
        Assert.Equal(usuarioMongoMock.Id, ((UsuarioMongo)result.Value).Id);
        Assert.Equal(usuarioMongoMock.Nombre, ((UsuarioMongo)result.Value).Nombre);
        Assert.Equal(usuarioMongoMock.Correo, ((UsuarioMongo)result.Value).Correo);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetUsuarioByIdQuery>(), default), Times.Once);
    }

    [Fact]
    public async Task GetUsuario_ShouldReturnNotFound_WhenUsuarioMongoDoesNotExist()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsuarioByIdQuery>(), default))
            .ReturnsAsync((UsuarioMongo?)null);

        // Act
        var result = await _controller.GetUsuario(usuarioId) as NotFoundResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetUsuarioByIdQuery>(), default), Times.Once);
    }
}