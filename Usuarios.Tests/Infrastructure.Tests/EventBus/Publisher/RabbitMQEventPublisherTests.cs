// RabbitMQEventPublisherTests.cs
using Domain.Events;
using Infrastructure.EventBus.Interfaces;
using Infrastructure.EventBus.Publisher;
using Moq;
using RabbitMQ.Client;
using System.Text.Json;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Publisher;

public class RabbitMQEventPublisherTests
{
    [Fact]
    public void Publish_ShouldDeclareExchange()
    {
        // Arrange
        var exchange = "user.exchange";
        var routingKey = "";

        var message = new UsuarioCreadoEvent(
            Guid.NewGuid(),
            "Juan",
            "Pérez",
            "juanp",
            "clave123",
            "123456789",
            "juan@example.com",
            "Calle Falsa 123",
            "ABC123"
        );

        var factoryMock = new Mock<IRabbitMQConnectionFactory>();
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IModel>();

        factoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
        connectionMock.Setup(c => c.CreateModel()).Returns(channelMock.Object);

        var publisher = new RabbitMQEventPublisher(factoryMock.Object);

        // Act
        publisher.Publish(message, exchange, routingKey);

        // Assert: verifica que se declaró el exchange
        channelMock.Verify(c => c.ExchangeDeclare(
            exchange,
            ExchangeType.Fanout,
            true,
            false,
            null
        ), Times.Once);
    }

    [Fact]
    public void SerializeMessage_ShouldIncludeTypeAndFields()
    {
        // Arrange
        var message = new UsuarioCreadoEvent(
            Guid.NewGuid(),
            "Juan",
            "Pérez",
            "juanp",
            "clave123",
            "123456789",
            "juan@example.com",
            "Calle Falsa 123",
            "ABC123"
        );

        // Act
        var body = RabbitMQEventPublisher.SerializeMessage(message);

        // Assert
        var json = System.Text.Encoding.UTF8.GetString(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(message.Id.ToString(), root.GetProperty("Id").GetString());
        Assert.Equal(message.Nombre, root.GetProperty("Nombre").GetString());
        Assert.Equal(message.Correo, root.GetProperty("Correo").GetString());
        Assert.Equal(nameof(UsuarioCreadoEvent), root.GetProperty("Type").GetString());
    }
}
