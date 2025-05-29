// Tests/RabbitMQConnectionFactoryTests.cs
using Moq;
using RabbitMQ.Client;
using Infrastructure.EventBus.Connection;
using Infrastructure.EventBus.Interfaces;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Connection;
public class RabbitMQConnectionFactoryTests
{
    [Fact]
    public void CreateConnection_ShouldReturnMockedConnection()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockFactoryWrapper = new Mock<IConnectionFactoryWrapper>();
        mockFactoryWrapper.Setup(f => f.CreateConnection()).Returns(mockConnection.Object);

        var factory = new RabbitMQConnectionFactory(mockFactoryWrapper.Object);

        // Act
        var connection = factory.CreateConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(mockConnection.Object, connection);
    }
}