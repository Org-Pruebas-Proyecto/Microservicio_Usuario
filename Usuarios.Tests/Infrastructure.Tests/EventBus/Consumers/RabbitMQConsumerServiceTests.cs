using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.EventBus.Consumers;
using Infrastructure.EventBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Consumers;

public class RabbitMQConsumerServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldConsumeMessageAndProcessIt()
    {
        // Arrange
        var messageProcessorMock = new Mock<IRabbitMQMessageProcessor>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        configurationMock.Setup(c => c["RabbitMQ:Username"]).Returns("guest");
        configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

        var channelMock = new Mock<IModel>();
        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(c => c.CreateModel()).Returns(channelMock.Object);

        var factoryMock = new Mock<IConnectionFactory>();
        factoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);

        var service = new RabbitMQConsumerService(configurationMock.Object, messageProcessorMock.Object, factoryMock.Object);

        // Simular que el consumidor llama al delegado
        AsyncEventingBasicConsumer capturedConsumer = null;
        channelMock
            .Setup(c => c.BasicConsume(It.IsAny<string>(), true, It.IsAny<string>(), false, false, null, It.IsAny<IBasicConsumer>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((queue, autoAck, consumerTag, noLocal, exclusive, args, consumer) =>
            {
                capturedConsumer = consumer as AsyncEventingBasicConsumer;
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Simular recepción de mensaje
        var body = Encoding.UTF8.GetBytes("{\"Type\":\"UsuarioRegistradoEvent\"}");
        var eventArgs = new BasicDeliverEventArgs
        {
            Body = new ReadOnlyMemory<byte>(body)
        };

        await capturedConsumer.HandleBasicDeliver("tag", 0, false, "exchange", "routingKey", null, eventArgs.Body);

        // Assert
        messageProcessorMock.Verify(p => p.ProcessMessageAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCloseChannelAndConnection()
    {
        // Arrange
        var messageProcessor = new Mock<IRabbitMQMessageProcessor>();
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        config.Setup(c => c["RabbitMQ:Username"]).Returns("guest");
        config.Setup(c => c["RabbitMQ:Password"]).Returns("guest");

        var channel = new Mock<IModel>();
        var connection = new Mock<IConnection>();
        connection.Setup(c => c.CreateModel()).Returns(channel.Object);

        var factory = new Mock<IConnectionFactory>();
        factory.Setup(f => f.CreateConnection()).Returns(connection.Object);

        var service = new RabbitMQConsumerService(config.Object, messageProcessor.Object, factory.Object);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        channel.Verify(c => c.Close(), Times.Once);
        connection.Verify(c => c.Close(), Times.Once);
    }
}
