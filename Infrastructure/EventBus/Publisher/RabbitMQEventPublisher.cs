// RabbitMQEventPublisher.cs
using Application.Interfaces;
using Infrastructure.EventBus.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.EventBus.Publisher;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQEventPublisher(IRabbitMQConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish<T>(T message, string exchangeName, string routingKey)
    {
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

        var body = SerializeMessage(message);
        _channel.BasicPublish(exchangeName, routingKey, null, (ReadOnlyMemory<byte>)body); // 👈 cast
    }

    public static byte[] SerializeMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var withType = new Dictionary<string, object>();
        foreach (var property in root.EnumerateObject())
        {
            withType[property.Name] = property.Value;
        }

        withType["Type"] = typeof(T).Name;

        var finalJson = JsonSerializer.Serialize(withType);
        return Encoding.UTF8.GetBytes(finalJson);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}