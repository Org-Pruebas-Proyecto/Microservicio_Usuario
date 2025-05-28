using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Queries;
using Domain.ValueObjects;

namespace Usuarios.Tests.Web.Tests.Controllers;



public class HistorialControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsuariosController _controller;

    public HistorialControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsuariosController(_mediatorMock.Object); // Simula el controlador
    }

    [Fact]
    public async Task ObtenerHistorial_ShouldReturnOk_WhenValidQuery()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var historialMock = new List<ActividadMongo>
        {
            new() { UsuarioId = usuarioId, TipoAccion = "Login", Fecha = DateTime.UtcNow },
            new() { UsuarioId = usuarioId, TipoAccion = "Logout", Fecha = DateTime.UtcNow.AddMinutes(-10) }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerHistorialQuery>(), default))
                     .ReturnsAsync(historialMock);

        // Act
        var result = await _controller.ObtenerHistorial(usuarioId, null, null, null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(historialMock, result.Value);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ObtenerHistorialQuery>(), default), Times.Once);
    }

    [Fact]
    public async Task ObtenerHistorial_ShouldReturnEmptyList_WhenNoData()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerHistorialQuery>(), default))
                     .ReturnsAsync(new List<ActividadMongo>());

        // Act
        var result = await _controller.ObtenerHistorial(usuarioId, null, null, null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Empty((IEnumerable<ActividadMongo>)result.Value);
    }
}