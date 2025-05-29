using Xunit;
using Moq;
using MongoDB.Driver;
using Infrastructure.Persistence.Repositories;
using Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Usuarios.Tests.Infrastructure.Tests.Persistance.Repositories;
public class MongoRepositoryTests
{
    [Fact]
    public async Task FindAsync_DeberiaRetornarResultados()
    {
        // Arrange
        var mockClient = new Mock<IMongoClient>();
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockCollection = new Mock<IMongoCollection<ActividadMongo>>();

        var actividades = new List<ActividadMongo>
        {
            new ActividadMongo
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                TipoAccion = "LOGIN",
                Detalles = "Inicio de sesión",
                Fecha = DateTime.UtcNow
            }
        };

        var asyncCursor = new Mock<IAsyncCursor<ActividadMongo>>();
        asyncCursor.Setup(_ => _.Current).Returns(actividades);
        asyncCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<ActividadMongo>>(),
                It.IsAny<FindOptions<ActividadMongo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(asyncCursor.Object);

        mockDatabase
            .Setup(d => d.GetCollection<ActividadMongo>("actividades", null))
            .Returns(mockCollection.Object);
        mockClient
            .Setup(c => c.GetDatabase("test_db", null))
            .Returns(mockDatabase.Object);

        var repository = new MongoRepository<ActividadMongo>(mockClient.Object, "test_db", "actividades");

        // Act
        var result = await repository.FindAsync(Builders<ActividadMongo>.Filter.Empty);

        // Assert
        Assert.Single(result);
    }
}
