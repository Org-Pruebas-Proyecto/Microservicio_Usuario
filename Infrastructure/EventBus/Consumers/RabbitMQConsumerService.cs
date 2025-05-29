using Infrastructure.EventBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.EventBus.Consumers;

public class RabbitMQConsumerService : IHostedService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IRabbitMQMessageProcessor _messageProcessor;

    private const string ExchangeName = "usuarios_exchange";
    private const string QueueName = "usuarios_mongo_queue";

    public RabbitMQConsumerService(
        IConfiguration configuration,
        IRabbitMQMessageProcessor messageProcessor)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"],
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"],
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _messageProcessor = messageProcessor;

        SetupRabbitMQ();
    }

    public RabbitMQConsumerService(
        IConfiguration configuration,
        IRabbitMQMessageProcessor messageProcessor,
        IConnectionFactory connectionFactory)
    {
        _messageProcessor = messageProcessor;

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        SetupRabbitMQ();
    }


    private void SetupRabbitMQ()
    {
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: "");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await _messageProcessor.ProcessMessageAsync(message);
        };

        _channel.BasicConsume(QueueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}
