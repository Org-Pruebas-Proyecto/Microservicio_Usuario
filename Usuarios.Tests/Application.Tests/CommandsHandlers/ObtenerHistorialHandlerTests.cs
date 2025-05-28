using Xunit;
using Moq;
using Application.Handlers;
using Application.Interfaces;
using Domain.ValueObjects;
using Application.Queries;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class ObtenerHistorialHandlerTests
{
    private readonly Mock<IMongoRepository<ActividadMongo>> _repositoryMock;
    private readonly ObtenerHistorialHandler _handler;

    public ObtenerHistorialHandlerTests()
    {
        _repositoryMock = new Mock<IMongoRepository<ActividadMongo>>();
        _handler = new ObtenerHistorialHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredResults_WhenValidQuery()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var actividades = new List<ActividadMongo>
        {
            new() { UsuarioId = usuarioId, TipoAccion = "Login", Fecha = DateTime.UtcNow },
            new() { UsuarioId = usuarioId, TipoAccion = "Logout", Fecha = DateTime.UtcNow.AddMinutes(-10) }
        };

        _repositoryMock.Setup(repo => repo.FindAsync(It.IsAny<FilterDefinition<ActividadMongo>>()))
                       .ReturnsAsync(actividades);

        var query = new ObtenerHistorialQuery(usuarioId, null, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(actividades.OrderByDescending(a => a.Fecha), result);
        _repositoryMock.Verify(repo => repo.FindAsync(It.IsAny<FilterDefinition<ActividadMongo>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoResults()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.FindAsync(It.IsAny<FilterDefinition<ActividadMongo>>()))
                       .ReturnsAsync(new List<ActividadMongo>());

        var query = new ObtenerHistorialQuery(Guid.NewGuid(), null, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _repositoryMock.Verify(repo => repo.FindAsync(It.IsAny<FilterDefinition<ActividadMongo>>()), Times.Once);
    }
}