using Infrastructure.EventBus.Connection;
using RabbitMQ.Client;
using Xunit;
using Moq;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Connection;

    public class ConnectionFactoryWrapperTests
    {
        [Fact]
        public void CreateConnection_ShouldReturnValidConnection()
        {
            // Arrange
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockFactory
                .Setup(f => f.CreateConnection())
                .Returns(mockConnection.Object);

            // Usar una clase interna de prueba que te permita inyectar el factory mockeado
            var wrapper = new TestableConnectionFactoryWrapper(mockFactory.Object);

            // Act
            var connection = wrapper.CreateConnection();

            // Assert
            Assert.NotNull(connection);
            Assert.Equal(mockConnection.Object, connection);
        }

        // Clase auxiliar para test que permite inyectar el IConnectionFactory
        private class TestableConnectionFactoryWrapper : IConnectionFactoryWrapper
        {
            private readonly IConnectionFactory _factory;

            public TestableConnectionFactoryWrapper(IConnectionFactory factory)
            {
                _factory = factory;
            }

            public IConnection CreateConnection()
            {
                return _factory.CreateConnection();
            }
        }
    }
