using System.Text.Json;
using System.Text;
using Application.Interfaces;
using RabbitMQ.Client;

namespace Infrastructure.EventBus.Events;

public class RabbitMQEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQEventPublisher(string host, string username, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish<T>(T message, string exchangeName, string routingKey)
    {
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
